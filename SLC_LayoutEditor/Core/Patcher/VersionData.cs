namespace SLC_LayoutEditor.Core.Patcher
{
    class VersionData
    {
        private readonly int major;
        private readonly int minor;
        private readonly int majorRevision;
        private readonly int minorRevision;

        private readonly int versionNumber;

        public int Major => major;

        public int Minor => minor;

        public int MajorRevision => majorRevision;

        public int MinorRevision => minorRevision;

        public int VersionNumber => versionNumber;

        public VersionData(string version)
        {
            string[] data = version.Split('.');

            for (int i = 0; i < data.Length; i++)
            {
                if (int.TryParse(data[i], out int part))
                {
                    switch (i)
                    {
                        case 0:
                            major = part;
                            break;
                        case 1:
                            minor = part;
                            break;
                        case 2:
                            majorRevision = part;
                            break;
                        case 3:
                            minorRevision = part;
                            break;
                    }
                }
            }

            versionNumber = PatcherUtil.ParseVersion(version, 0);
        }

        public bool IsOlder(string newVersionString)
        {
            VersionData newVersion = new VersionData(newVersionString);

            if (newVersion.minorRevision > minorRevision &&
                newVersion.majorRevision >= majorRevision &&
                newVersion.minor >= minor &&
                newVersion.major >= major)
            {
                return true;
            }
            else if (newVersion.majorRevision > majorRevision &&
                newVersion.minor >= minor &&
                newVersion.major >= major)
            {
                return true;
            }
            else if (newVersion.minor > minor &&
                newVersion.major >= major)
            {
                return true;
            }
            else if (newVersion.major > major)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
