﻿using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace Oxide.Game.SpaceEngineers
{
    /// <summary>
    /// Game commands for the core Space Engineers plugin
    /// </summary>
    public partial class SpaceEngineersCore : CSPlugin
    {
        #region Grant Command

        /// <summary>
        /// Called when the "grant" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("GrantCommand")]
        private void GrantCommand(IPlayer player, string command, string[] args)
        {
            if (!PermissionsLoaded(player)) return;

            if (args.Length < 3)
            {
                player.Reply(lang.GetMessage("CommandUsageGrant", this, player.Id));
                return;
            }

            var mode = args[0];
            var name = args[1];
            var perm = args[2];

            if (!permission.PermissionExists(perm))
            {
                player.Reply(lang.GetMessage("PermissionNotFound", this, player.Id), perm);
                return;
            }

            if (mode.Equals("group"))
            {
                if (!permission.GroupExists(name))
                {
                    player.Reply(lang.GetMessage("GroupNotFound", this, player.Id), name);
                    return;
                }

                if (permission.GroupHasPermission(name, perm))
                {
                    player.Reply(lang.GetMessage("GroupAlreadyHasPermission", this, player.Id), name, perm);
                    return;
                }

                permission.GrantGroupPermission(name, perm, null);
                player.Reply(lang.GetMessage("GroupPermissionGranted", this, player.Id), name, perm);
            }
            else if (mode.Equals("user"))
            {
                var target = Covalence.PlayerManager.FindPlayer(name);
                if (target == null && !permission.UserIdValid(name))
                {
                    player.Reply(lang.GetMessage("UserNotFound", this, player.Id), name);
                    return;
                }

                var userId = name;
                if (target != null)
                {
                    userId = target.Id;
                    name = target.Name;
                    permission.UpdateNickname(userId, name);
                }

                if (permission.UserHasPermission(name, perm))
                {
                    player.Reply(lang.GetMessage("UserAlreadyHasPermission", this, player.Id), userId, perm);
                    return;
                }

                permission.GrantUserPermission(userId, perm, null);
                player.Reply(lang.GetMessage("UserPermissionGranted", this, player.Id), $"{name} ({userId})", perm);
            }
            else player.Reply(lang.GetMessage("CommandUsageGrant", this, player.Id));
        }

        #endregion

        // TODO: GrantAllCommand (grant all permissions from user(s)/group(s))
 
        #region Group Command

        /// <summary>
        /// Called when the "group" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("GroupCommand")]
        private void GroupCommand(IPlayer player, string command, string[] args)
        {
            if (!PermissionsLoaded(player)) return;

            if (args.Length < 2)
            {
                player.Reply(lang.GetMessage("CommandUsageGroup", this, player.Id));
                player.Reply(lang.GetMessage("CommandUsageGroupParent", this, player.Id));
                player.Reply(lang.GetMessage("CommandUsageGroupRemove", this, player.Id));
                return;
            }

            var mode = args[0];
            var group = args[1];
            var title = args.Length >= 3 ? args[2] : "";
            var rank = args.Length == 4 ? int.Parse(args[3]) : 0;

            if (mode.Equals("add"))
            {
                if (permission.GroupExists(group))
                {
                    player.Reply(lang.GetMessage("GroupAlreadyExists", this, player.Id), group);
                    return;
                }

                permission.CreateGroup(group, title, rank);
                player.Reply(lang.GetMessage("GroupCreated", this, player.Id), group);
            }
            else if (mode.Equals("remove"))
            {
                if (!permission.GroupExists(group))
                {
                    player.Reply(lang.GetMessage("GroupNotFound", this, player.Id), group);
                    return;
                }

                permission.RemoveGroup(group);
                player.Reply(lang.GetMessage("GroupDeleted", this, player.Id), group);
            }
            else if (mode.Equals("set"))
            {
                if (!permission.GroupExists(group))
                {
                    player.Reply(lang.GetMessage("GroupNotFound", this, player.Id), group);
                    return;
                }

                permission.SetGroupTitle(group, title);
                permission.SetGroupRank(group, rank);
                player.Reply(lang.GetMessage("GroupChanged", this, player.Id), group);
            }
            else if (mode.Equals("parent"))
            {
                if (args.Length <= 2)
                {
                    player.Reply(lang.GetMessage("CommandUsageGroupParent", this, player.Id));
                    return;
                }

                if (!permission.GroupExists(group))
                {
                    player.Reply(lang.GetMessage("GroupNotFound", this, player.Id), group);
                    return;
                }

                var parent = args[2];
                if (!string.IsNullOrEmpty(parent) && !permission.GroupExists(parent))
                {
                    player.Reply(lang.GetMessage("GroupParentNotFound", this, player.Id), parent);
                    return;
                }

                if (permission.SetGroupParent(group, parent))
                    player.Reply(lang.GetMessage("GroupParentChanged", this, player.Id), group, parent);
                else
                    player.Reply(lang.GetMessage("GroupParentNotChanged", this, player.Id), group);
            }
            else
            {
                player.Reply(lang.GetMessage("CommandUsageGroup", this, player.Id));
                player.Reply(lang.GetMessage("CommandUsageGroupParent", this, player.Id));
                player.Reply(lang.GetMessage("CommandUsageGroupRemove", this, player.Id));
            }
        }

        #endregion

        #region Lang Command

        /// <summary>
        /// Called when the "lang" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("LangCommand")]
        private void LangCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply(lang.GetMessage("CommandUsageLang", this, player.Id));
                return;
            }

            if (player.IsServer)
            {
                // TODO: Check if langauge exists before setting, warn if not
                lang.SetServerLanguage(args[0]);
                player.Reply(lang.GetMessage("ServerLanguage", this, player.Id), lang.GetServerLanguage());
            }
            else
            {
                // TODO: Check if langauge exists before setting, warn if not
                var languages = lang.GetLanguages(null);
                if (languages.Contains(args[0])) lang.SetLanguage(args[0], player.Id);
                player.Reply(lang.GetMessage("PlayerLanguage", this, player.Id), args[0]);
            }
        }

        #endregion

        #region Load Command

        /// <summary>
        /// Called when the "load" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("LoadCommand")]
        private void LoadCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply(lang.GetMessage("CommandUsageLoad", this, player.Id));
                return;
            }

            if (args[0].Equals("*") || args[0].Equals("all"))
            {
                Interface.Oxide.LoadAllPlugins();
                return;
            }

            foreach (var name in args)
            {
                if (string.IsNullOrEmpty(name)) continue;
                Interface.Oxide.LoadPlugin(name);
                pluginManager.GetPlugin(name);
            }
        }

        #endregion

        #region Plugins Command

        /// <summary>
        /// Called when the "plugins" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("PluginsCommand")]
        private void PluginsCommand(IPlayer player, string command, string[] args)
        {
            var loadedPlugins = pluginManager.GetPlugins().Where(pl => !pl.IsCorePlugin).ToArray();
            var loadedPluginNames = new HashSet<string>(loadedPlugins.Select(pl => pl.Name));
            var unloadedPluginErrors = new Dictionary<string, string>();
            foreach (var loader in Interface.Oxide.GetPluginLoaders())
            {
                foreach (var name in loader.ScanDirectory(Interface.Oxide.PluginDirectory).Except(loadedPluginNames))
                {
                    string msg;
                    unloadedPluginErrors[name] = (loader.PluginErrors.TryGetValue(name, out msg)) ? msg : "Unloaded"; // TODO: Localization
                }
            }

            var totalPluginCount = loadedPlugins.Length + unloadedPluginErrors.Count;
            if (totalPluginCount < 1)
            {
                player.Reply(lang.GetMessage("NoPluginsFound", this, player.Id));
                return;
            }

            var output = $"Listing {loadedPlugins.Length + unloadedPluginErrors.Count} plugins:"; // TODO: Localization
            var number = 1;
            foreach (var plugin in loadedPlugins.Where(p => p.Filename != null))
                output += $"\n  {number++:00} \"{plugin.Title}\" ({plugin.Version}) by {plugin.Author} ({plugin.TotalHookTime:0.00}s) - {plugin.Filename.Basename()}";
            foreach (var pluginName in unloadedPluginErrors.Keys)
                output += $"\n  {number++:00} {pluginName} - {unloadedPluginErrors[pluginName]}";
            player.Reply(output);
        }

        #endregion

        #region Reload Command

        /// <summary>
        /// Called when the "reload" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("ReloadCommand")]
        private void ReloadCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply(lang.GetMessage("CommandUsageReload", this, player.Id));
                return;
            }

            if (args[0].Equals("*") || args[0].Equals("all"))
            {
                Interface.Oxide.ReloadAllPlugins();
                return;
            }

            foreach (var name in args)
                if (!string.IsNullOrEmpty(name)) Interface.Oxide.ReloadPlugin(name);
        }

        #endregion

        #region Revoke Command

        /// <summary>
        /// Called when the "revoke" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("RevokeCommand")]
        private void RevokeCommand(IPlayer player, string command, string[] args)
        {
            if (!PermissionsLoaded(player)) return;

            if (args.Length < 3)
            {
                player.Reply(lang.GetMessage("CommandUsageRevoke", this, player.Id));
                return;
            }

            var mode = args[0];
            var name = args[1];
            var perm = args[2];

            if (mode.Equals("group"))
            {
                if (!permission.GroupExists(name))
                {
                    player.Reply(lang.GetMessage("GroupNotFound", this, player.Id), name);
                    return;
                }

                if (!permission.GroupHasPermission(name, perm))
                {
                    // TODO: Check if group is inheriting permission, mention
                    player.Reply(lang.GetMessage("GroupDoesNotHavePermission", this, player.Id), name, perm);
                    return;
                }

                permission.RevokeGroupPermission(name, perm);
                player.Reply(lang.GetMessage("GroupPermissionRevoked", this, player.Id), name, perm);
            }
            else if (mode.Equals("user"))
            {
                var target = Covalence.PlayerManager.FindPlayer(name);
                if (target == null && !permission.UserIdValid(name))
                {
                    player.Reply(lang.GetMessage("UserNotFound", this, player.Id), name);
                    return;
                }

                var userId = name;
                if (target != null)
                {
                    userId = target.Id;
                    name = target.Name;
                    permission.UpdateNickname(userId, name);
                }

                if (!permission.UserHasPermission(userId, perm))
                {
                    // TODO: Check if user is inheriting permission, mention
                    player.Reply(lang.GetMessage("UserDoesNotHavePermission", this, player.Id), name, perm);
                    return;
                }

                permission.RevokeUserPermission(userId, perm);
                player.Reply(lang.GetMessage("UserPermissionRevoked", this, player.Id), $"{name} ({userId})", perm);
            }
            else player.Reply(lang.GetMessage("CommandUsageRevoke", this, player.Id));
        }

        #endregion

        // TODO: RevokeAllCommand (revoke all permissions from user(s)/group(s))

        #region Show Command

        /// <summary>
        /// Called when the "show" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("ShowCommand")]
        private void ShowCommand(IPlayer player, string command, string[] args)
        {
            if (!PermissionsLoaded(player)) return;

            if (args.Length < 1)
            {
                player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
                player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
                return;
            }

            var mode = args[0];
            var name = args.Length == 2 ? args[1] : string.Empty;

            if (mode.Equals("perms"))
            {
                player.Reply(lang.GetMessage("Permissions", this, player.Id) + ":\n" + string.Join(", ", permission.GetPermissions()));
            }
            else if (mode.Equals("perm"))
            {
                if (args.Length < 2 || string.IsNullOrEmpty(name))
                {
                    player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
                    player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
                    return;
                }

                var users = permission.GetPermissionUsers(name);
                var groups = permission.GetPermissionGroups(name);
                var result = $"{string.Format(lang.GetMessage("PermissionUsers", this, player.Id), name)}:\n";
                result += users.Length > 0 ? string.Join(", ", users) : lang.GetMessage("NoPermissionUsers", this, player.Id);
                result += $"\n\n{string.Format(lang.GetMessage("PermissionGroups", this, player.Id), name)}:\n";
                result += groups.Length > 0 ? string.Join(", ", groups) : lang.GetMessage("NoPermissionGroups", this, player.Id);
                player.Reply(result);
            }
            else if (mode.Equals("user"))
            {
                if (args.Length < 2 || string.IsNullOrEmpty(name))
                {
                    player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
                    player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
                    return;
                }

                var target = Covalence.PlayerManager.FindPlayer(name);
                if (target == null && !permission.UserIdValid(name))
                {
                    player.Reply(lang.GetMessage("UserNotFound", this, player.Id), name);
                    return;
                }
                var userId = name;
                if (target != null)
                {
                    userId = target.Id;
                    name = target.Name;
                    permission.UpdateNickname(userId, name);
                    name += $" ({userId})";
                }

                var perms = permission.GetUserPermissions(userId);
                var groups = permission.GetUserGroups(userId);
                var result = $"{string.Format(lang.GetMessage("UserPermissions", this, player.Id), name)}:\n";
                result += perms.Length > 0 ? string.Join(", ", perms) : lang.GetMessage("NoUserPermissions", this, player.Id);
                result += $"\n\n{string.Format(lang.GetMessage("UserGroups", this, player.Id), name)}:\n";
                result += groups.Length > 0 ? string.Join(", ", groups) : lang.GetMessage("NoUserGroups", this, player.Id);
                player.Reply(result);
            }
            else if (mode.Equals("group"))
            {
                if (args.Length < 2 || string.IsNullOrEmpty(name))
                {
                    player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
                    player.Reply(lang.GetMessage("CommandUsageShowName", this, player.Id));
                    return;
                }
                
                if (!permission.GroupExists(name))
                {
                    player.Reply(lang.GetMessage("GroupNotFound", this, player.Id), name);
                    return;
                }

                var users = permission.GetUsersInGroup(name);
                var perms = permission.GetGroupPermissions(name);
                var result = $"{string.Format(lang.GetMessage("GroupUsers", this, player.Id), name)}:\n";
                result += users.Length > 0 ? string.Join(", ", users) : lang.GetMessage("NoUsersInGroup", this, player.Id);
                result += $"\n\n{string.Format(lang.GetMessage("GroupPermissions", this, player.Id), name)}:\n";
                result += perms.Length > 0 ? string.Join(", ", perms) : lang.GetMessage("NoGroupPermissions", this, player.Id);
                var parent = permission.GetGroupParent(name);
                while (permission.GroupExists(parent))
                {
                    result += $"\n{string.Format(lang.GetMessage("ParentGroupPermissions", this, player.Id), parent)}:\n";
                    result += string.Join(", ", permission.GetGroupPermissions(parent));
                    parent = permission.GetGroupParent(parent);
                }
                player.Reply(result);
            }
            else if (mode.Equals("groups"))
            {
                player.Reply(lang.GetMessage("Groups", this, player.Id) + ":\n" + string.Join(", ", permission.GetGroups()));
            }
            else player.Reply(lang.GetMessage("CommandUsageShow", this, player.Id));
        }

        #endregion

        #region Unload Command

        /// <summary>
        /// Called when the "unload" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("UnloadCommand")]
        private void UnloadCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply(lang.GetMessage("CommandUsageUnload", this, player.Id));
                return;
            }

            if (args[0].Equals("*") || args[0].Equals("all"))
            {
                Interface.Oxide.UnloadAllPlugins();
                return;
            }

            foreach (var name in args)
                if (!string.IsNullOrEmpty(name)) Interface.Oxide.UnloadPlugin(name);
        }

        #endregion
 
        #region User Group Command

        /// <summary>
        /// Called when the "usergroup" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("UserGroupCommand")]
        private void UserGroupCommand(IPlayer player, string command, string[] args)
        {
            if (!PermissionsLoaded(player)) return;

            if (args.Length < 3)
            {
                player.Reply(lang.GetMessage("CommandUsageUserGroup", this, player.Id));
                return;
            }

            var mode = args[0];
            var name = args[1];
            var group = args[2];

            var target = Covalence.PlayerManager.FindPlayer(name);
            if (target == null && !permission.UserIdValid(name))
            {
                player.Reply(lang.GetMessage("UserNotFound", this, player.Id), name);
                return;
            }
            var userId = name;
            if (target != null)
            {
                userId = target.Id;
                name = target.Name;
                permission.UpdateNickname(userId, name);
                name += $"({userId})";
            }

            if (!permission.GroupExists(group))
            {
                player.Reply(lang.GetMessage("GroupNotFound", this, player.Id), group);
                return;
            }

            if (mode.Equals("add"))
            {
                permission.AddUserGroup(userId, group);
                player.Reply(lang.GetMessage("UserAddedToGroup", this, player.Id), name, group);
            }
            else if (mode.Equals("remove"))
            {
                permission.RemoveUserGroup(userId, group);
                player.Reply(lang.GetMessage("UserRemovedFromGroup", this, player.Id), name, group);
            }
            else player.Reply(lang.GetMessage("CommandUsageUserGroup", this, player.Id));
        }

        #endregion

        // TODO: UserGroupAllCommand (add/remove all users to/from group)

        #region Version Command

        /// <summary>
        /// Called when the "version" command has been executed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        [HookMethod("VersionCommand")]
        private void VersionCommand(IPlayer player, string command, string[] args)
        {
            if (player.IsServer)
            {
                /*player.Reply($"Protocol: {Server.Protocol}\nBuild Date: {BuildInfo.Current.BuildDate}\n" +
                $"Unity Version: {UnityEngine.Application.unityVersion}\nChangeset: {BuildInfo.Current.Scm.ChangeId}\n" +
                $"Branch: {BuildInfo.Current.Scm.Branch}\nOxide Version: {OxideMod.Version}");*/ // TODO: Implement server version
            }
            else
            {
                var format = Covalence.FormatText(lang.GetMessage("Version", this, player.Id));
                player.Reply(format, OxideMod.Version, Covalence.GameName, Server.Version, Server.Protocol);
            }
        }

        #endregion
    }
}
