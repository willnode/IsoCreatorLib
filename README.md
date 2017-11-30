# IsoCreatorLib

IsoCreatorLib is a library edition of [original ISO Creator by Bunny Eating Rabbit on SourceForge](https://sourceforge.net/projects/iso-creator-cs/).

In a nutshell this repo is the fork of the original repo.

## Usage

This is a .NET Library for creating [ISO 9660 Joliet](https://en.wikipedia.org/wiki/ISO_9660) Files. Able to build files from stored folder or virtual and run in separate thread (with progress reporting).

## Changes

+ GUI Code Removed (build as a library)
+ Uses C# 7.0 Feature and Syntax
+ Redundant Code Removed (in attempt to make the code more readable)
+ Namespace changed to `IsoCreatorLib`

## Demo

Pack a folder and its contents to ISO (synchronous):

```c#
new IsoCreator().Folder2Iso(@"C:\Path\To\Folder", @"C:\TargetIso.iso", "VOLUME_NAME");
```

Pack a folder and its contents to ISO (asynchronous):

```c#
// Hook up events before starting the process on another thread.

var creator = new IsoCreator();
creator.Progress += delegate (object sender, ProgressEventArgs e) {
    Console.WriteLine($"{e.Action}: {e.Current} of {e.Maximum}");
};

creator.Finish += delegate (object sender, FinishEventArgs e) {
    Console.WriteLine($"FINISHED: {e.Message}");
};

creator.Abort += delegate (object sender, AbortEventArgs e) {
    Console.WriteLine($"ABORTED: {e.Message}");
};

new Thread(new ParameterizedThreadStart(creator.Folder2Iso))
    .Start(new IsoCreatorFolderArgs(@"C:\Path\To\Folder", @"C:\TargetIso.iso", "VOLUME_NAME"));
```

For GUI and Virtual Folder Demo please refer to the original repo.

## LICENSE

Uhh. I will contacting Bunny Eating Rabbit for clarification on this.