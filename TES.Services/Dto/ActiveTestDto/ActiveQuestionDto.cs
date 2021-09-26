﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain.Helpers;

namespace TES.Services.Dto.ActiveTestDto
{
    public class ActiveQuestionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public QuestionType QuestionType { get; set; }
        public int WorthOfPoints { get; set; }
    }
}
