﻿using GameInterface.Serialization;
using System.Collections.Generic;
using Xunit;
using TaleWorlds.CampaignSystem;
using GameInterface.Serialization.Native;
using Common.Serialization;

namespace GameInterface.Tests.Serialization.SerializerTests
{
    public class DictionarySerializerTest
    {
        [Fact]
        public void Dictionary_Serialize()
        {
            Dictionary<string, CampaignTime> dict = new Dictionary<string, CampaignTime>
            {
                { "1", new CampaignTime() },
                { "2", new CampaignTime() },
                { "3", new CampaignTime() },
            };

            BinaryPackageFactory factory = new BinaryPackageFactory();
            DictionaryBinaryPackage package = new DictionaryBinaryPackage(dict, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void Dictionary_Full_Serialization()
        {
            Dictionary<string, CampaignTime> dict = new Dictionary<string, CampaignTime>
            {
                { "1", new CampaignTime() },
                { "2", new CampaignTime() },
                { "3", new CampaignTime() },
            };

            BinaryPackageFactory factory = new BinaryPackageFactory();
            DictionaryBinaryPackage package = new DictionaryBinaryPackage(dict, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);

            object obj = BinaryFormatterSerializer.Deserialize(bytes);

            Assert.IsType<DictionaryBinaryPackage>(obj);

            DictionaryBinaryPackage returnedPackage = (DictionaryBinaryPackage)obj;

            Dictionary<string, CampaignTime> newDict = returnedPackage.Unpack<Dictionary<string, CampaignTime>>();

            Assert.Equal(dict, newDict);
        }
    }
}
