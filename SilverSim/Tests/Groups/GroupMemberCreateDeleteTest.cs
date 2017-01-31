// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

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
        UUI m_Founder = new UUI("11111111-2222-3333-4444-555555555555", "Group", "Creator");
        UUI m_Invitee = new UUI("55555555-4444-3333-2222-111111111111", "Group", "Invitee");
        UUID m_GroupID = new UUID("11223344-1122-1122-1122-112233445566");
        UUID m_InsigniaID = new UUID("11223344-1122-1122-1122-112233445577");

        public GroupMemberCreateDeleteTest()
        {

        }

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
            List<string> unequal = new List<string>();
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
            GroupInfo gInfo;
            GroupInfo testGroupInfo;

            gInfo = new GroupInfo();
            gInfo.Charter = "Charter";
            gInfo.Founder = m_Founder;
            gInfo.ID.ID = m_GroupID;
            gInfo.ID.GroupName = "Test Group";
            gInfo.InsigniaID = m_InsigniaID;
            gInfo.IsAllowPublish = true;
            gInfo.IsMaturePublish = false;
            gInfo.IsOpenEnrollment = true;
            gInfo.IsShownInList = false;
            gInfo.MembershipFee = 10;

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
