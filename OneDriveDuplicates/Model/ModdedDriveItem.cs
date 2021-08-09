using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveDuplicates.Model
{
    public class ModdedDriveItem 
    {
        public DriveItem File;
        public string Hash { get; set; }
        public string Name => File.Name;
        public string Id => File.Id;
        public ItemReference ParentReference => File.ParentReference;

        public ModdedDriveItem(DriveItem item)
        {
            this.File = item;
            if (item.File.Hashes != null)
            {
                if (!string.IsNullOrWhiteSpace(item.File.Hashes.Sha256Hash))
                {
                    Hash = item.File.Hashes.Sha256Hash;
                } 
                else if (!string.IsNullOrWhiteSpace(item.File.Hashes.Sha1Hash))
                {
                    Hash = item.File.Hashes.Sha1Hash;
                } 
                else if (!string.IsNullOrWhiteSpace(item.File.Hashes.QuickXorHash))
                {
                    Hash = item.File.Hashes.QuickXorHash;
                }
                else
                {
                    Debug.WriteLine($"{item.Name} has not the right hashes.");
                }
            }
            else
            {
                Debug.WriteLine($"{item.Name} has no hashes.");
            }
        }
    }
}
