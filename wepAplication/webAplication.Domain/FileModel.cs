﻿public class FileModel
{
    public string Id { get { return id; } set { } }
    private string id = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Path { get; set; }
}