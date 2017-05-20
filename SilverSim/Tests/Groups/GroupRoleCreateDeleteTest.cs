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
    public class GroupRoleCreateDeleteTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        string m_GroupsServiceName;
        string m_BackendGroupsServiceName;
        string m_AvatarNameServiceName;
        GroupsServiceInterface m_GroupsService;
        GroupsServiceInterface m_BackendGroupsService;
        AvatarNameServiceInterface m_AvatarNameService;
        UUI m_Founder = new UUI("11111111-2222-3333-4444-555555555555", "Group", "Creator");
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
            m_AvatarNameService.Store(m_Founder);
        }

        bool CheckEveryoneRole(GroupRole role, UGI group)
        {
            var unequal = new List<string>();
            if (role.ID != UUID.Zero)
            {
                unequal.Add("ID");
            }

            if (role.Group != group)
            {
                unequal.Add(string.Format("Group ({0}!={1})", role.Group.ToString(), group.ToString()));
            }

            if (role.Members != 1)
            {
                unequal.Add("Members");
            }

            if (role.Name != "Everyone")
            {
                unequal.Add("Name");
            }

            if (role.Description != "Everyone in the group")
            {
                unequal.Add("Description");
            }

            if (role.Powers != GroupPowers.DefaultEveryonePowers)
            {
                unequal.Add("Powers");
            }

            if (role.Title != "Member of Test Group")
            {
                unequal.Add("Title");
            }

            if (unequal.Count != 0)
            {
                m_Log.InfoFormat("Data mismatch: {0}", string.Join(",", unequal));
                return false;
            }
            return true;
        }

        bool CheckOwnersRole(GroupRole role, UGI group, UUID roleID)
        {
            var unequal = new List<string>();
            if (role.ID != roleID)
            {
                unequal.Add("ID");
            }

            if (role.Group != group)
            {
                unequal.Add(string.Format("Group ({0}!={1})", role.Group.ToString(), group.ToString()));
            }

            if (role.Members != 1)
            {
                unequal.Add("Members");
            }

            if (role.Name != "Owners")
            {
                unequal.Add("Name");
            }

            if (role.Description != "Owners of the group")
            {
                unequal.Add("Description");
            }

            if (role.Powers != GroupPowers.OwnerPowers)
            {
                unequal.Add("Powers");
            }

            if (role.Title != "Owner of Test Group")
            {
                unequal.Add("Title");
            }

            if (unequal.Count != 0)
            {
                m_Log.InfoFormat("Data mismatch: {0}", string.Join(",", unequal));
                return false;
            }
            return true;
        }

        public bool Run()
        {
            GroupInfo gInfo;
            GroupInfo testGroupInfo;
            UGI ugi;
            GroupRole role;

            m_Log.Info("Checking for group non-existence 1");
            try
            {
                gInfo = m_GroupsService.Groups[m_Founder, "Test Group"];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* intentionally ignored */
            }

            m_Log.Info("Checking for group non-existence 2");
            try
            {
                ugi = m_GroupsService.Groups[m_Founder, m_GroupID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* intentionally ignored */
            }

            m_Log.Info("Checking for group non-existence 3");
            try
            {
                gInfo = m_GroupsService.Groups[m_Founder, new UGI(m_GroupID)];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* intentionally ignored */
            }

            m_Log.Info("Checking for group non-existence 4");
            if (m_GroupsService.Groups.TryGetValue(m_Founder, "Test Group", out gInfo))
            {
                return false;
            }

            m_Log.Info("Checking for group non-existence 5");
            if (m_GroupsService.Groups.TryGetValue(m_Founder, m_GroupID, out ugi))
            {
                return false;
            }
            m_Log.Info("Checking for group non-existence 6");
            if (m_GroupsService.Groups.TryGetValue(m_Founder, new UGI(m_GroupID), out gInfo))
            {
                return false;
            }

            gInfo = new GroupInfo()
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

            m_Log.Info("Testing role Everyone existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), UUID.Zero, out role))
            {
                return false;
            }
            if (!CheckEveryoneRole(role, testGroupInfo.ID))
            {
                return false;
            }

            m_Log.Info("Testing role Owners existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), testGroupInfo.OwnerRoleID, out role))
            {
                return false;
            }
            if (!CheckOwnersRole(role, testGroupInfo.ID, testGroupInfo.OwnerRoleID))
            {
                return false;
            }

            var testrole = new GroupRole()
            {
                Group = testGroupInfo.ID,
                Name = "Test Role",
                Description = "Test Description",
                ID = UUID.Random,
                Title = "Test",
                Powers = GroupPowers.DefaultEveryonePowers
            };
            m_Log.Info("Testing role Test non-existence");
            if (m_GroupsService.Roles.ContainsKey(m_Founder, new UGI(m_GroupID), testrole.ID))
            {
                return false;
            }

            m_Log.Info("Creating role Test");
            m_GroupsService.Roles.Add(m_Founder, testrole);

            m_Log.Info("Testing role Test existence");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), testrole.ID, out role))
            {
                return false;
            }

            {
                var unequal = new List<string>();
                if(role.Group != testrole.Group)
                {
                    unequal.Add("Group");
                }
                if(role.ID != testrole.ID)
                {
                    unequal.Add("ID");
                }
                if(role.Name != testrole.Name)
                {
                    unequal.Add("Name");
                }
                if(role.Description != testrole.Description)
                {
                    unequal.Add("Description");
                }
                if(role.Powers != testrole.Powers)
                {
                    unequal.Add("Powers");
                }
                if(role.Title != testrole.Title)
                {
                    unequal.Add("Title");
                }
                if(unequal.Count != 0)
                {
                    m_Log.InfoFormat("Data mismatch: {0}", string.Join(" ", unequal));
                    return false;
                }
            }

            m_Log.Info("Testing role Everyone existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), UUID.Zero, out role))
            {
                return false;
            }
            if (!CheckEveryoneRole(role, testGroupInfo.ID))
            {
                return false;
            }

            m_Log.Info("Testing role Owners existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), testGroupInfo.OwnerRoleID, out role))
            {
                return false;
            }
            if (!CheckOwnersRole(role, testGroupInfo.ID, testGroupInfo.OwnerRoleID))
            {
                return false;
            }

            m_Log.Info("Check group for role count");
            GroupInfo compareInfo;
            if(!m_GroupsService.Groups.TryGetValue(m_Founder, new UGI(m_GroupID), out compareInfo))
            {
                return false;
            }

            if(compareInfo.RoleCount != 3)
            {
                m_Log.Info("New role test is not seen as part of RoleCount");
                return false;
            }

            m_Log.Info("Testing role Test existence");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), testrole.ID, out role))
            {
                return false;
            }

            {
                var unequal = new List<string>();
                if (role.Group != testrole.Group)
                {
                    unequal.Add("Group");
                }
                if (role.ID != testrole.ID)
                {
                    unequal.Add("ID");
                }
                if (role.Name != testrole.Name)
                {
                    unequal.Add("Name");
                }
                if (role.Description != testrole.Description)
                {
                    unequal.Add("Description");
                }
                if (role.Powers != testrole.Powers)
                {
                    unequal.Add("Powers");
                }
                if (role.Title != testrole.Title)
                {
                    unequal.Add("Title");
                }
                if (unequal.Count != 0)
                {
                    m_Log.InfoFormat("Data mismatch: {0}", string.Join(" ", unequal));
                    return false;
                }
            }

            m_Log.Info("Testing role Everyone existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), UUID.Zero, out role))
            {
                return false;
            }
            if (!CheckEveryoneRole(role, testGroupInfo.ID))
            {
                return false;
            }

            m_Log.Info("Testing role Owners existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), testGroupInfo.OwnerRoleID, out role))
            {
                return false;
            }
            if (!CheckOwnersRole(role, testGroupInfo.ID, testGroupInfo.OwnerRoleID))
            {
                return false;
            }

            m_Log.Info("Check group for role count");
            if (!m_GroupsService.Groups.TryGetValue(m_Founder, new UGI(m_GroupID), out compareInfo))
            {
                return false;
            }

            if (compareInfo.RoleCount != 3)
            {
                m_Log.Info("New role test is not seen as part of RoleCount");
                return false;
            }

            m_Log.Info("Updating role Test");
            testrole.Name = "Test Role 2";
            testrole.Description = "Test Description 2";
            testrole.Title = "Test Title 2";
            testrole.Powers |= GroupPowers.AssignMember;
            m_GroupsService.Roles.Add(m_Founder, testrole);

            m_Log.Info("Deleting role Test");
            m_GroupsService.Roles.Delete(m_Founder, new UGI(m_GroupID), testrole.ID);

            m_Log.Info("Testing role Everyone existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), UUID.Zero, out role))
            {
                return false;
            }
            if (!CheckEveryoneRole(role, testGroupInfo.ID))
            {
                return false;
            }

            m_Log.Info("Testing role Owners existence 1");
            if (!m_GroupsService.Roles.TryGetValue(m_Founder, new UGI(m_GroupID), testGroupInfo.OwnerRoleID, out role))
            {
                return false;
            }
            if (!CheckOwnersRole(role, testGroupInfo.ID, testGroupInfo.OwnerRoleID))
            {
                return false;
            }

            m_Log.Info("Check group for role count");
            if (!m_GroupsService.Groups.TryGetValue(m_Founder, new UGI(m_GroupID), out compareInfo))
            {
                return false;
            }

            if (compareInfo.RoleCount != 2)
            {
                m_Log.Info("Only two roles should be seen after deletion");
                return false;
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