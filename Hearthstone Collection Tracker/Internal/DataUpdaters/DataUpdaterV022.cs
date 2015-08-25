﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV022 : IDataUpdater
    {
        private static readonly Version _version = new Version(0, 2, 2);

        public Version Version
        {
            get
            {
                return _version;
            }
        }

        private string ConfigFilePath
        {
            get { return Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "config.xml"); }
        }

        public bool RequiresUpdate
        {
            get
            {
                var configFilePath = ConfigFilePath;
                if (!Directory.Exists(HearthstoneCollectionTrackerPlugin.PluginDataDir) || !File.Exists(configFilePath))
                {
                    return false;
                }

                try
                {
                    var settings = Hearthstone_Deck_Tracker.XmlManager<PluginSettings>.Load(configFilePath);
                    return settings.CurrentVersion < new ModuleVersion(_version);
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
        }

        private const string DreadscaleCardId = "AT_063t";

        public void PerformUpdate()
        {
            var configFilePath = ConfigFilePath;
            var settings = Hearthstone_Deck_Tracker.XmlManager<PluginSettings>.Load(configFilePath);
            // remove extra Dreadscale copy from each account
            foreach (var account in settings.Accounts)
            {
                if (!File.Exists(account.FileStoragePath))
                {
                    continue;
                }
                var setsInfo = Hearthstone_Deck_Tracker.XmlManager<List<BasicSetCollectionInfo>>.Load(account.FileStoragePath);
                var TGTSet = setsInfo.FirstOrDefault(s => s.SetName == "The Grand Tournament");
                if (TGTSet == null)
                {
                    continue;
                }
                var dreadScaleCards = TGTSet.Cards.Where(c => c.CardId == DreadscaleCardId).ToList();
                if (dreadScaleCards.Count > 1)
                {
                    TGTSet.Cards = TGTSet.Cards.Where(c => c.CardId != DreadscaleCardId).ToList();
                    TGTSet.Cards.Add(dreadScaleCards.First());

                    Hearthstone_Deck_Tracker.XmlManager<List<BasicSetCollectionInfo>>.Save(account.FileStoragePath, setsInfo);
                }
            }
            settings.CurrentVersion = new ModuleVersion(_version);
            Hearthstone_Deck_Tracker.XmlManager<PluginSettings>.Save(configFilePath, settings);
        }

        [Serializable]
        public class PluginSettings
        {
            public ModuleVersion CurrentVersion { get; set; }

            public string ActiveAccount { get; set; }

            public List<AccountSummary> Accounts { get; set; }

            public double CollectionWindowWidth { get; set; }

            public double CollectionWindowHeight { get; set; }

            public bool DefaultShowAllCards { get; set; }

            public bool NotifyNewDeckMissingCards { get; set; }
        }

        [Serializable]
        public class AccountSummary
        {
            public string AccountName { get; set; }

            public string FileStoragePath { get; set; }
        }

        public class BasicSetCollectionInfo
        {
            public string SetName { get; set; }

            public List<CardInCollection> Cards { get; set; }
        }

        public class CardInCollection
        {
            public int AmountNonGolden { get; set; }

            public int AmountGolden { get; set; }

            public int DesiredAmount { get; set; }

            public string CardId { get; set; }
        }
    }
}