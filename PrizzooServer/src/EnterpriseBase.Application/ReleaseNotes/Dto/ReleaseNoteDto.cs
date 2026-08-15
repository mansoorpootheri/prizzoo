using System;
using System.Collections.Generic;

namespace EnterpriseBase.Application.ReleaseNotes.Dto
{
    public class ReleaseNoteDto
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Features { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class CreateReleaseNoteDto
    {
        public string Version { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Features { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateReleaseNoteDto
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Features { get; set; }
        public bool IsActive { get; set; }
    }
}
