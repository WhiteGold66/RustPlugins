using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Core.Libraries.Covalence;
using Oxide.Plugins;

namespace MyPlugin
{
    [Info("Deforest", "Banter", "0.1.0")]
    [Description("Deforests an area within a given radius")]
    class Deforest : RustPlugin
    {
        // The cooldown period in seconds
        const int COOLDOWN_PERIOD = 10;

        // The radius of the deforested area in meters
        const float RADIUS = 10.0f;

        // A dictionary to store the last time the deforest command was used for each player
        Dictionary<ulong, float> lastUsed = new Dictionary<ulong, float>();

        void Init()
        {
            // Register the "deforest" command
            AddCovalenceCommand("deforest", nameof(DeforestCommand));
        }

        void DeforestCommand(IPlayer player, string command, string[] args)
        {
            // Check if the player has permission to use the command
            if (!player.HasPermission("deforest.use"))
            {
                player.Reply("You do not have permission to use this command.");
                return;
            }

            // Check if the player is on cooldown
            if (IsOnCooldown(player.Id))
            {
                player.Reply("You must wait before using this command again.");
                return;
            }

            // Deforest the area
            DeforestArea(player.Object as BasePlayer);

            // Add the player to the cooldown list
            AddToCooldown(player.Id);
        }

        bool IsOnCooldown(ulong playerId)
        {
            // Check if the player is in the dictionary
            if (!lastUsed.ContainsKey(playerId))
            {
                return false;
            }

            // Get the current time
            float currentTime = Time.realtimeSinceStartup;

            // Check if the cooldown period has elapsed
            return currentTime - lastUsed[playerId] < COOLDOWN_PERIOD;
        }

        void AddToCooldown(ulong playerId)
        {
            // Update the last used time for the player
            lastUsed[playerId] = Time.realtimeSinceStartup;
        }

        void DeforestArea(BasePlayer player)
        {
            // Get the player's position
            Vector3 playerPos = player.transform.position;

            // Get all the trees within the radius
            List<TreeInstance> trees = TerrainMeta.ForestMap.AllTrees;
            List<TreeInstance> treesInRange = trees.Where(x => Vector3.Distance(x.Position, playerPos) <= RADIUS).ToList();

            // Destroy each tree
            foreach (TreeInstance tree in treesInRange)
            {
                TerrainMeta.ForestMap.RemoveTree(tree.PrototypeIndex, tree.Position);
            }
        }
    }
}
