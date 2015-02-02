﻿namespace NamespaceRenamer
{
    public class Conflict
    {
        public string Message { get; set; }
        public Types MessageType { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
    }
    public enum Types { conflict, warning, adding, delimiter };
}