﻿using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility;
using ModKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ToyBox {
    // To be clear this is an editor of your list of saves
    // ToyBox already takes care of the role of the actual save editor
    public static class GameSavesBrowser {
        public static Settings Settings => Main.Settings;
        private static Browser<SaveInfo, SaveInfo> savesBrowser = new(true, true);
        private static (string, string) nameEditState = (null, null);
        private static List<SaveInfo> _allSaves = null;
        private static List<SaveInfo> _currentSaves = null;
        public static string? SearchKey(this SaveInfo info) =>
            $"{info.Name}{info.Area.AreaName}{info.Campaign.Title}{info.DlcCampaign.Campaign.Title}{info.Description}{info.FileName}";
        public static IComparable?[] SortKey(this SaveInfo info) => [
            info.PlayerCharacterName ?? info.Name ?? info.FileName,
            info.GameSaveTime
        ];

        public static void OnGUI() {
            var saveManager = Game.Instance?.SaveManager;
            string? currentGameID = Game.Instance?.Player?.GameId;

            Div(0, 25);
            HStack("Saves".localize(),
                   1,
                   () => {
                       Toggle("Auto load Last Save on launch".localize(), ref Settings.toggleAutomaticallyLoadLastSave, 500.width());
                       HelpLabel("Hold down shift during launch to bypass".localize());
                   },
                   () => {
                       using (HorizontalScope()) {
                           Label("Save ID: ".localize());
                           if (currentGameID != null) {
                               if (EditableLabel(ref currentGameID, ref nameEditState, 100)) {
                                   Game.Instance.Player.GameId = currentGameID;
                               }
                           } else {
                               currentGameID = "N/A".localize();
                               Label(currentGameID);
                           }
                       }
                   },
                   () => { }
            );
            if (Main.IsInGame) {
                Div(50, 25);
                //var currentSave = Game.Instance.SaveManager.GetLatestSave();
                // TODO: add refresh
                if (_currentSaves == null || _allSaves == null) {
                    saveManager?.UpdateSaveListIfNeeded(true);
                    _currentSaves = saveManager?.Where(info => info?.GameId == currentGameID).ToList();
                    _allSaves = saveManager?.m_SavedGames.NotNull().ToList();
                }
                if (_currentSaves == null || _allSaves == null) {
                    return;
                }
                using (VerticalScope()) {
                    savesBrowser.OnGUI(_currentSaves,
                                       () => _allSaves,
                                       info => info,
                                       info => info.SearchKey(),
                                       info => info.SortKey(),
                                       () => {
                                           Toggle("Show GameID".localize(), ref Settings.toggleShowGameIDs);
                                       },
                                       (info, _) => {
                                           var isCurrent = _currentSaves.Contains(info);
                                           var characterName = isCurrent ? info.PlayerCharacterName.Orange() : info.PlayerCharacterName;
                                           Label(characterName, 400.width());
                                           25.space();
                                           Label($"{info.Area.AreaName.StringValue()}".Cyan(), 400.width());
                                           if (Settings.toggleShowGameIDs) {
                                               25.space();
                                               ClipboardLabel(info.GameId, 400.width());
                                           }
                                           25.space();
                                           HelpLabel(info.Name.ToString());
                                       }, null, 50, true, true, 100, 400);
                }
            }

        }
    }
}
