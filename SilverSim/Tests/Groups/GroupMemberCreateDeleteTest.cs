// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3 with
// the following clarification and special exception.

// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU Affero General Public License cover the whole
// combination.

// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.AvatarName;
using SilverSim.ServiceInterfaces.Groups;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Groups;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Groups
{
    public class GroupMemberCreateDeleteTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        string m_GroupsServiceName;
        string m_BackendGroupsServiceName;
        string m_AvatarNameServiceName;
        GroupsServiceInterface m_GroupsService;
        GroupsServiceInterface m_BackendGroupsService;
        AvatarNameServiceInterface m_AvatarNameService;
        UGUIWithName m_Founder = new UGUIWithName("11111111-2222-3333-4444-555555555555", "Group", "Creator");
        UGUIWithName m_Invitee = new UGUIWithName("55555555-4444-3333-2222-111111111111", "Group", "Invitee");
        UUID m_GroupID = new UUID("11223344-1122-1122-1122-112233445566");
        UUID m_InsigniaID = new UUID("11223344-1122-1122-1122-112233445577");

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_GroupsServiceName = config.GetString("GroupsService");
            m_BackendGroupsServiceName = config.GetString("BackendGroupsService", m_GroupsServiceName);
            m_AvatarNameServiceName = config.GetString("AvatarNameService");

            m_GroupsService = loader.GetService<GroupsServiceInterface>(m_GroupsServiceName);
            m_BackendGroupsService = loader.GetService<GroupsServiceInterface>(m_BackendGroupsServiceName);
            m_AvatarNameService = loader.GetService<AvatarNameServiceInterface>(m_AvatarNameServiceName);
        }

        public void Setup()
        {
            /* intentionally left empty */
            m_AvatarNameService.Store(m_Founder);
            m_AvatarNameService.Store(m_Invitee);
        }

        bool CheckEquality(GroupMember mem1, GroupMember mem2)
        {
            var unequal = new List<string>();
            if(mem1.AccessToken != mem2.AccessToken)
            {
                unequal.Add("AccessToken");
            }
            if(mem1.Contribution != mem2.Contribution)
            {
                unequal.Add("Contribution");
            }
            if(mem1.Group != mem2.Group)
            {
                unequal.Add("Group");
            }
            if(mem1.IsAcceptNotices != mem2.IsAcceptNotices)
            {
                unequal.Add("IsAcceptNotices");
            }
            if(mem1.IsListInProfile != mem2.IsListInProfile)
            {
                unequal.Add("IsListInProfile");
            }
            if(mem1.Principal != mem2.Principal)
            {
                unequal.Add(string.Format("Principal {0}!={1}", mem1.Principal.ToString(), mem2.Principal.ToString()));
            }
            if(mem1.SelectedRoleID != mem2.SelectedRoleID)
            {
                unequal.Add("SelectedRoleID");
            }

            if(unequal.Count != 0)
            {
                m_Log.InfoFormat("Data mismatch {0}", string.Join(" ", unequal));
                return false;
            }
            return true;
        }

        public bool Run()
        {
            GroupInfo testGroupInfo;

            var gInfo = new GroupInfo()
            {
                Charter = "Charter",
                Founder = m_Founder,
                ID = new UGI { ID = m_GroupID, GroupName = "Test Group" },
                InsigniaID = m_InsigniaID,
                IsAllowPublish = true,
                IsMaturePublish = false,
                IsOpenEnrollment = true,
                IsShownInList = false,
                MembershipFee = 10
            };
            m_Log.Info("Creating group");
            testGroupInfo = m_GroupsService.CreateGroup(m_Founder, gInfo, GroupPowers.DefaultEveryonePowers, GroupPowers.OwnerPowers);
            m_GroupID = testGroupInfo.ID.ID;

            m_Log.Info("Checking for group existence");
            gInfo = m_GroupsService.Groups[m_Founder, "Test Group"];

            m_Log.Info("Adding new member");
            GroupMember newMember = m_GroupsService.Members.Add(m_Founder, new UGI(m_GroupID), m_Invitee, UUID.Zero, UUID.Random.ToString());

            m_Log.Info("Testing existence of new member 1");
            if (!m_GroupsService.Members.ContainsKey(m_Founder, new UGI(m_GroupID), m_Invitee))
            {
                return false;
            }

            m_Log.Info("Testing existence of new member 2");
            GroupMember testMember;
            if(!m_GroupsService.Members.TryGetValue(m_Founder, new UGI(m_GroupID), m_Invitee, out testMember))
            {
                return false;
            }

            if(!CheckEquality(newMember, testMember))
            {
                return false;
            }

            m_Log.Info("Testing existence of new member 3");
            try
            {
                testMember = m_GroupsService.Members[m_Founder, new UGI(m_GroupID), m_Invitee];
            }
            catch(KeyNotFoundException)
            {
                return false;
            }

            if (!CheckEquality(newMember, testMember))
            {
                return false;
            }

            m_Log.Info("Deleting new member");
            m_GroupsService.Members.Delete(m_Founder, new UGI(m_GroupID), m_Invitee);

            m_Log.Info("Testing non-existence of new member 1");
            if (m_GroupsService.Members.ContainsKey(m_Founder, new UGI(m_GroupID), m_Invitee))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of new member 2");
            if (m_GroupsService.Members.TryGetValue(m_Founder, new UGI(m_GroupID), m_Invitee, out testMember))
            {
                return false;
            }

            m_Log.Info("Testing existence of new member 3");
            try
            {
                testMember = m_GroupsService.Members[m_Founder, new UGI(m_GroupID), m_Invitee];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* intentionally ignored */
            }

            return true;
        }

        public void Cleanup()
        {
            try
            {
                m_BackendGroupsService.Groups.Delete(m_Founder, new UGI(m_GroupID));
            }
            catch
            {
                /* intentionally ignored */
            }
        }
    }
}
