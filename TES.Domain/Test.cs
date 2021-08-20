using System;
using System.Collections.Generic;
using TES.Domain.Helpers;

namespace TES.Domain
{
    public class Test 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; }
        public TestType TestType { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public List<TestResult> Results { get; set; }
        public TestLink UrlLinkId { get; set; }

    }
}
