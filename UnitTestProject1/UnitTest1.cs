using Common.Models.Seo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CanDeserializeScoreBreakdown()
        {
            var text = @"[{""field"": ""title"", ""score"": 100, ""issues"": [], ""details"": {""length"": 60}}, {""field"": ""meta"", ""score"": 100, ""issues"": [], ""details"": {""length"": 154}}, {""field"": ""content"", ""score"": 100, ""issues"": [], ""details"": {""word_count"": 1532, ""has_headings"": true}}, {""field"": ""images"", ""score"": 50, ""issues"": [""Missing alt text""], ""details"": {""has_image"": true, ""has_alt"": false}}, {""field"": ""technical"", ""score"": 100, ""issues"": [], ""details"": {""url_length"": 69}}, {""field"": ""keywords"", ""score"": 0, ""issues"": [""No target keyword defined""], ""details"": {}}]";
            var breakdown = JsonSerializer.Deserialize<ScoreBreakdown[]>(text);
        }
    }
}
