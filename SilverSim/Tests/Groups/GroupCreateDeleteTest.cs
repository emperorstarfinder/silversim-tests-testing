﻿// SilverSim is distributed under the terms of the
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
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Groups
{
    public class GroupCreateDeleteTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        string m_GroupsServiceName;
        string m_BackendGroupsServiceName;
        string m_AvatarNameServiceName;
        GroupsServiceInterface m_GroupsService;
        GroupsServiceInterface m_BackendGroupsService;
        AvatarNameServiceInterface m_AvatarNameService;
        UGUIWithName m_Founder = new UGUIWithName("11111111-2222-3333-4444-555555555555", "Group", "Creator");
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

        bool CheckForEquality(GroupInfo gInfo, GroupInfo testGroupInfo)
        {
            var unequal = new List<string>();

            if (!gInfo.ID.Equals(testGroupInfo.ID))
            {
                unequal.Add("GroupID");
            }

            if (gInfo.Charter != testGroupInfo.Charter)
            {
                unequal.Add("Charter");
            }

            if(gInfo.Founder != testGroupInfo.Founder)
            {
                unequal.Add(string.Format("Founder ({0}!={1})", gInfo.Founder.ToString(), testGroupInfo.Founder.ToString()));
            }

            if(gInfo.InsigniaID != testGroupInfo.InsigniaID)
            {
                unequal.Add("InsigniaID");
            }

            if(gInfo.IsAllowPublish != testGroupInfo.IsAllowPublish)
            {
                unequal.Add("IsAllowPublish");
            }

            if(gInfo.IsMaturePublish != testGroupInfo.IsMaturePublish)
            {
                unequal.Add("IsMaturePublish");
            }

            if(gInfo.IsOpenEnrollment != testGroupInfo.IsOpenEnrollment)
            {
                unequal.Add("IsOpenEnrollment");
            }

            if(gInfo.IsShownInList != testGroupInfo.IsShownInList)
            {
                unequal.Add("IsShownInList");
            }

            if(gInfo.OwnerRoleID != testGroupInfo.OwnerRoleID)
            {
                unequal.Add("OwnerRoleID");
            }

            if(gInfo.MemberCount != 1)
            {
                unequal.Add("MemberCount");
            }

            if(gInfo.RoleCount != 2)
            {
                unequal.Add("RoleCount");
                return false;
            }

            if (unequal.Count != 0)
            {
                m_Log.InfoFormat("Data mismatch: {0}", string.Join(" ", unequal));
                return false;
            }
            return true;
        }

        bool CheckEveryoneRole(GroupRole role, UGI group)
        {
            var unequal = new List<string>();
            if(role.ID != UUID.Zero)
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

            if(role.Title != "Member of Test Group")
            {
                unequal.Add("Title");
            }

            if(unequal.Count != 0)
            {
                m_Log.InfoFormat("Data mismatch: {0}", string.Join(",", unequal));
                return false;
            }
            return true;
        }

        bool CheckOwnersRole(GroupRole role, UGI group,UUID roleID)
        {
            var unequal = new List<string>();
            if(role.ID != roleID)
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
            catch(KeyNotFoundException)
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
            if(m_GroupsService.Groups.TryGetValue(m_Founder, m_GroupID, out ugi))
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

            m_Log.Info("Checking for group existence 1");
            gInfo = m_GroupsService.Groups[m_Founder, "Test Group"];

            if (!CheckForEquality(gInfo, testGroupInfo))
            {
                return false;
            }

            m_Log.Info("Checking for group existence 2");
            ugi = m_GroupsService.Groups[m_Founder, m_GroupID];
            if(ugi != testGroupInfo.ID)
            {
                return false;
            }

            m_Log.Info("Checking for group existence 3");
            gInfo = m_GroupsService.Groups[m_Founder, new UGI(m_GroupID)];
            if (!CheckForEquality(gInfo, testGroupInfo))
            {
                return false;
            }

            m_Log.Info("Checking for group existence 4");
            if (!m_GroupsService.Groups.TryGetValue(m_Founder, "Test Group", out gInfo))
            {
                return false;
            }
            if (!CheckForEquality(gInfo, testGroupInfo))
            {
                return false;
            }

            m_Log.Info("Checking for group existence 5");
            if (!m_GroupsService.Groups.TryGetValue(m_Founder, m_GroupID, out ugi))
            {
                return false;
            }
            if(ugi != testGroupInfo.ID)
            {
                return false;
            }

            m_Log.Info("Checking for group existence 6");
            if (!m_GroupsService.Groups.TryGetValue(m_Founder, new UGI(m_GroupID), out gInfo))
            {
                return false;
            }
            if (!CheckForEquality(gInfo, testGroupInfo))
            {
                return false;
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

            m_Log.Info("Testing role Everyone existence 2");
            role = m_GroupsService.Roles[m_Founder, new UGI(m_GroupID), UUID.Zero];
            
            if(!CheckEveryoneRole(role, testGroupInfo.ID))
            {
                return false;
            }

            m_Log.Info("Testing role Everyone existence 3");
            if(!m_GroupsService.Roles.ContainsKey(m_Founder, new UGI(m_GroupID), UUID.Zero))
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

            m_Log.Info("Testing role Owners existence 2");
            role = m_GroupsService.Roles[m_Founder, new UGI(m_GroupID), testGroupInfo.OwnerRoleID];

            if (!CheckOwnersRole(role, testGroupInfo.ID, testGroupInfo.OwnerRoleID))
            {
                return false;
            }

            m_Log.Info("Testing role Owners existence 3");
            if (!m_GroupsService.Roles.ContainsKey(m_Founder, new UGI(m_GroupID), testGroupInfo.OwnerRoleID))
            {
                return false;
            }

            try
            {
                m_Log.Info("Delete group");
                m_BackendGroupsService.Groups.Delete(m_Founder, testGroupInfo.ID);
            }
            catch(NotSupportedException)
            {
                return true;
            }

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
