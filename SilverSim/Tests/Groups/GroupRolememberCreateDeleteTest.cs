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
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Groups
{
    public class GroupRolememberCreateDeleteTest : ITest
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

        public GroupRolememberCreateDeleteTest()
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

            GroupInvite invite;
            UUID inviteID = UUID.Random;

            m_Log.Info("Testing non-existence of invite 1");
            if (m_GroupsService.Invites.ContainsKey(m_Founder, inviteID))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of invite 2");
            if (m_GroupsService.Invites.TryGetValue(m_Founder, inviteID, out invite))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of invite 3");
            try
            {
                invite = m_GroupsService.Invites[m_Founder, inviteID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* intentionally ignored */
            }

            m_Log.Info("Testing non-existence of invite 4");
            try
            {
                if (m_GroupsService.Invites[m_Founder, new UGI(m_GroupID), gInfo.OwnerRoleID, m_Invitee].Count != 0)
                {
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Testing non-existence of invite 5");
            try
            {
                if (m_GroupsService.Invites[m_Founder, m_Invitee].Count != 0)
                {
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Testing non-existence of invite 6");
            try
            {
                if (m_GroupsService.Invites.GetByGroup(m_Founder, new UGI(m_GroupID)).Count != 0)
                {
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Creating invite");
            GroupInvite testinvite = new GroupInvite();
            testinvite.ID = inviteID;
            testinvite.Principal = m_Invitee;
            testinvite.RoleID = gInfo.OwnerRoleID;
            testinvite.Group = new UGI(m_GroupID);
            m_GroupsService.Invites.Add(m_Founder, testinvite);

            m_Log.Info("Testing existence of invite 1");
            if (!m_GroupsService.Invites.ContainsKey(m_Founder, inviteID))
            {
                return false;
            }

            m_Log.Info("Testing existence of invite 2");
            if (!m_GroupsService.Invites.TryGetValue(m_Founder, inviteID, out invite))
            {
                return false;
            }

            if (invite.ID != testinvite.ID ||
                invite.Principal != testinvite.Principal ||
                invite.RoleID != testinvite.RoleID ||
                invite.Group != testinvite.Group)
            {
                m_Log.Info("Data mismatch");
                return false;
            }

            m_Log.Info("Testing existence of invite 3");
            invite = m_GroupsService.Invites[m_Founder, inviteID];

            if (invite.ID != testinvite.ID ||
                invite.Principal != testinvite.Principal ||
                invite.RoleID != testinvite.RoleID ||
                invite.Group != testinvite.Group)
            {
                m_Log.Info("Data mismatch");
                return false;
            }

            m_Log.Info("Testing existence of invite 4");
            List<GroupInvite> invites;
            try
            {
                if ((invites = m_GroupsService.Invites[m_Founder, new UGI(m_GroupID), gInfo.OwnerRoleID, m_Invitee]).Count != 1)
                {
                    return false;
                }

                invite = invites[0];
                if (invite.ID != testinvite.ID ||
                    invite.Principal != testinvite.Principal ||
                    invite.RoleID != testinvite.RoleID ||
                    invite.Group != testinvite.Group)
                {
                    m_Log.Info("Data mismatch");
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Testing non-existence of invite 5");
            try
            {
                if ((invites = m_GroupsService.Invites[m_Founder, m_Invitee]).Count != 1)
                {
                    return false;
                }

                invite = invites[0];
                if (invite.ID != testinvite.ID ||
                    invite.Principal != testinvite.Principal ||
                    invite.RoleID != testinvite.RoleID ||
                    invite.Group != testinvite.Group)
                {
                    m_Log.Info("Data mismatch");
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Testing non-existence of invite 6");
            try
            {
                if ((invites = m_GroupsService.Invites.GetByGroup(m_Founder, new UGI(m_GroupID))).Count != 1)
                {
                    return false;
                }

                invite = invites[0];
                if (invite.ID != testinvite.ID ||
                    invite.Principal != testinvite.Principal ||
                    invite.RoleID != testinvite.RoleID ||
                    invite.Group != testinvite.Group)
                {
                    m_Log.Info("Data mismatch");
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Deleting role Test");
            m_GroupsService.Invites.Delete(m_Founder, testinvite.ID);

            m_Log.Info("Testing non-existence of invite 1");
            if (m_GroupsService.Invites.ContainsKey(m_Founder, inviteID))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of invite 2");
            if (m_GroupsService.Invites.TryGetValue(m_Founder, inviteID, out invite))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of invite 3");
            try
            {
                invite = m_GroupsService.Invites[m_Founder, inviteID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* intentionally ignored */
            }

            m_Log.Info("Testing non-existence of invite 4");
            try
            {
                if (m_GroupsService.Invites[m_Founder, new UGI(m_GroupID), gInfo.OwnerRoleID, m_Invitee].Count != 0)
                {
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Testing non-existence of invite 5");
            try
            {
                if (m_GroupsService.Invites[m_Founder, m_Invitee].Count != 0)
                {
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
            }

            m_Log.Info("Testing non-existence of invite 6");
            try
            {
                if (m_GroupsService.Invites.GetByGroup(m_Founder, new UGI(m_GroupID)).Count != 0)
                {
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                m_Log.Info("not supported");
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
