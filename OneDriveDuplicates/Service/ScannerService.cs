using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;

namespace OneDriveDuplicates.Service
{
    public class ScannerService
    {
        GraphServiceClient graphClient;
        public ScannerService(GraphServiceClient client)
        {
            graphClient = client;
        }

        public async IAsyncEnumerable<DriveItem> GetFolders(string id = null)
        {
            IDriveItemChildrenCollectionPage items = null;
            try
            {
                IDriveItemChildrenCollectionRequest request = null;
                // Get the items
                if (id == null)
                {
                    request = graphClient.Me.Drive.Root.Children.Request();
                }
                else
                {
                    request = graphClient.Me.Drive.Items[id].Children.Request();
                }
                
                items = await request.Select(x => new { x.Id, x.Folder, x.Name, x.ParentReference })
                    //.Filter("folder ne null")
                    .Top(50)
                    .GetAsync();
            }
            catch
            {
                items = null;
            }

            if (items != null)
            {
                foreach (var item in items?.CurrentPage)
                {
                    if (item.Folder != null)
                        yield return item;
                }

                while (items.NextPageRequest != null)
                {
                    try
                    {
                        items = await items.NextPageRequest.GetAsync();
                    }
                    catch
                    {
                        items = null;
                    }
                    foreach (var item in items?.CurrentPage)
                    {
                        if (item.Folder != null)
                            yield return item;
                    }
                }
            }
        }

        public async Task<DriveItem> GetFolderAsync(string id)
        {
            try
            {
                var result = await graphClient.Me.Drive.Items[id].Request()
                    .Select(x => x.WebUrl)
                    .GetAsync();
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async IAsyncEnumerable<DriveItem> GetFilesInFolder(string folderId=null, bool includeSubFolders=false)
        {
            List<DriveItem> folders = new List<DriveItem>();
            IDriveItemChildrenCollectionPage items = null;
            try
            {

                IDriveItemChildrenCollectionRequest request = null;

                if (folderId == null)
                {
                    request = graphClient.Me.Drive.Root.Children.Request();
                }
                else
                {
                    request = graphClient.Me.Drive.Items[folderId].Children.Request();
                }

                if (!includeSubFolders)
                {
                    request = request.Select(x => new { x.Id, x.File, x.Name, x.ParentReference });
                }
                else
                {
                    request = request.Select(x => new { x.Id, x.File, x.Folder, x.Name, x.ParentReference, x.CreatedDateTime, x.Size });
                }

                items = await request.Top(50)
                    .GetAsync();
            }
            catch
            {
                items = null;
            }

            if (items != null)
            {
                foreach (var item in items.CurrentPage)
                {
                    if (item.File != null)
                        yield return item;
                    else if (item.Folder != null)
                        folders.Add(item);
                }

                while (items.NextPageRequest != null)
                {
                    items = await items.NextPageRequest.GetAsync();
                    foreach (var item in items.CurrentPage)
                    {
                        if (item.File != null)
                            yield return item;
                        else if (item.Folder != null)
                            folders.Add(item);
                    }
                }
            }

            if (includeSubFolders)
            {
                foreach (var folder in folders)
                {
                    Debug.WriteLine($"Checking folder: {folder.Name}");
                    await foreach (var file in GetFilesInFolder(folder.Id, includeSubFolders))
                    {
                        yield return file;
                    }
                }
            }

        }

        public async Task DeleteBatchFileAsync(List<string> ids)
        {
            var sequence = ids.AsEnumerable();

            while (sequence.Any())
            {
                var batch = sequence.Take(20);
                sequence = sequence.Skip(20);

                // do whatever you need to do with each batch here

                var batchRequestContent = new BatchRequestContent();
                int batchStep = 1;
                foreach (var id in batch)
                {
                    var request = graphClient.Me.Drive.Items[id].Request();
                    var rawRequest = new HttpRequestMessage(HttpMethod.Delete, request.RequestUrl);
                    batchRequestContent.AddBatchRequestStep(new BatchRequestStep(batchStep.ToString(), rawRequest));
                    batchStep++;
                }
                
                await graphClient.Batch.Request().PostAsync(batchRequestContent);
            }
        }

        public Task DeleteFileAsync(string id)
        {
            var batchRequestContent = new BatchRequestContent();
            
            return graphClient.Me.Drive.Items[id].Request().DeleteAsync();
        }

        //public async IAsyncEnumerable<DriveItem> GetFiles()
        //{
        //    List<DriveItem> folders = new List<DriveItem>();
        //    IDriveItemChildrenCollectionPage items = null;
        //    try
        //    {
                
        //        // Get the items
        //        items = await graphClient.Me.Drive.Root.Children.Request()
        //            .Select(x => new { x.Id, x.File, x.Folder, x.Name, x.ParentReference })
        //            .Top(50)
        //            .GetAsync();
        //    }
        //    catch
        //    {
        //        var i = 3;
        //    }

        //    foreach (var item in items.CurrentPage)
        //    {
        //        if (item.File != null)
        //            yield return item;
        //        else if (item.Folder != null)
        //            folders.Add(item);
        //    }

        //    while (items.NextPageRequest != null)
        //    {
        //        items = await items.NextPageRequest.GetAsync();
        //        foreach (var item in items.CurrentPage)
        //        {
        //            if (item.File != null)
        //                yield return item;
        //            else if (item.Folder != null)
        //                folders.Add(item);
        //        }
        //    }

        //    foreach (var folder in folders)
        //    {
        //        Debug.WriteLine($"Checking folder: {folder.Name}");
        //        await foreach (var file in GetFilesFromFolder(folder))
        //        {
        //            yield return file;
        //        }
        //    }
            
        //}

        //private async IAsyncEnumerable<DriveItem> GetFilesFromFolder(DriveItem folder)
        //{
        //    IDriveItemChildrenCollectionPage items = null;
        //    List<DriveItem> folders = new List<DriveItem>();
        //    try
        //    {
        //        await Task.Delay(500);
        //        // Get the items
        //        items = await graphClient.Me.Drive.Items[folder.Id].Children.Request()
        //            .Select(x => new { x.Id, x.File, x.Folder, x.Name, x.ParentReference, x.Size })
        //            .Top(50)
        //            .GetAsync();
        //    }
        //    catch
        //    {
        //        var i = 3;
        //    }

        //    foreach (var item in items.CurrentPage)
        //    {
        //        if (item.File != null)
        //            yield return item;
        //        else if (item.Folder != null)
        //            folders.Add(item);
        //    }

        //    while (items.NextPageRequest != null)
        //    {
        //        items = await items.NextPageRequest.GetAsync();
        //        foreach (var item in items.CurrentPage)
        //        {
        //            if (item.File != null)
        //                yield return item;
        //            else if (item.Folder != null)
        //                folders.Add(item);
        //        }
        //    }

        //    foreach (var subFolder in folders)
        //    {
        //        Debug.WriteLine($"Checking folder: {subFolder.Name}");
        //        await foreach (var file in GetFilesFromFolder(subFolder))
        //        {
        //            yield return file;
        //        }
        //    }
        //}
    }
}
