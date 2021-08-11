# OneDriveDuplicates

This repo is a simple UWP app I made to find duplicate files in a OneDrive account **without** downloading the files to your drive.  

It does this via comparing the hashes present for each file.  For now, it will just keep the file that has the shorter name. i.e. photo.jpg vs photo (1).jpg will keep the first one.  For now, comparisons are only made within a folder, but you can search sub folders at the same time.

This app is setup to be able to do multiple pages, but I have the NavigationMenu disabled for now.  I would be happy to take pull requests to make this more functional.  But it has worked for me quite well.  My old photos (from when I had a Lumia) had sync errors so I had several duplicates of the same photos.  This found them all and deleted them without deleting the original. 
