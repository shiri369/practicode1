//fib bundle --output D:\folder\bundlefile.txt

using System.CommandLine;

var bundlecomand = new Command("bundle", "bundle code files to a single file Use 'all' to include all code files in the directory.");  //bundle

var bundleoption = new Option<FileInfo>(new[] { "--output", "-o" }, "file path and name");  //option to bandle comand
var languageoption = new Option<string[]>(new[] { "--language", "-l" }, "Unification of the selected language files")
{
    IsRequired = true,
};
var authorOption = new Option<string>(new[] { "--author", "-a" }, "The author of the bundle");
var empty_linesoption = new Option<bool>(new[] { "--remove", "-r" }, "remove-empty-lines");
var noteoption = new Option<bool>(new[] { "--note", "-n" }, "Do list the source of the file");
var sortoption = new Option<string>(new[] { "--sort", "-s" }, () => "name", "sort by names fule or code");


bundlecomand.AddOption(languageoption);
bundlecomand.AddOption(bundleoption); // הוספה האופציה לבנדל
bundlecomand.AddOption(noteoption);
bundlecomand.AddOption(sortoption);
bundlecomand.AddOption(empty_linesoption);
bundlecomand.AddOption(authorOption);

bundlecomand.SetHandler((output, language, note,sort,remove,author) => //עריכת הפקודה output
{
  

    var allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), ".");    //מקבלת את כל הקבצים של התיקיה הנוכחית 
    //sort
    // מיון המערך לפי השם או הסיומת
    if (sort == "name")
    {
        allFiles = allFiles.OrderBy(file => Path.GetFileName(file)).ToArray();
    }
    else if (sort == "language")
    {
        allFiles = allFiles.OrderBy(file => Path.GetExtension(file).ToLower()).ToArray();
    }

    // הגדרת תיקיות שיש להחריג (כגון bin, obj, debug, release)
    var excludedDirectories = new[] { "bin", "obj", "debug", "release" };

    // סינון הקבצים של המערך כך שלא יכללו קבצים שנמצאים בתיקיות המוחרגות
    allFiles = allFiles
        .Where(file => !excludedDirectories.Any(dir => file.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar)))
        .ToArray();


    string bundleFilePath = output.FullName;//פתיחת קובץ
    using (StreamWriter writer = new StreamWriter(bundleFilePath, true)) // true מאפשר הוספה
    {
        if (!string.IsNullOrEmpty(author))
    {
        writer.WriteLine($"// Author: {author}");
    }
    //בדיקה עבור כל קובץ האם הוא הוא השפה שנבחרה אם כן אקח אותו 
        for (var i = 0; i < allFiles.Length; i++)
        {
            string extension = Path.GetExtension(allFiles[i]).ToLower().TrimStart('.');
        
        string languages = extension switch
        {
            "cs" => "c#",
            "py" => "python",
            "js" => "javascript",
            "html" => "html",
            "css" => "css",
            _ => "unknown" // אם השפה לא ידועה
        };
            if (language.Contains("all") || language.Contains(languages))
            {
                Console.WriteLine($"Including file: {Path.GetFileName(allFiles[i])} with language: {language}");

                 var lines = File.ReadAllLines(allFiles[i]);
                //note
                //האם לכתוב את מקור הקובץ
                if (note)
                {
                    string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), allFiles[i]);
                    writer.WriteLine($"// Source: {Path.GetFileName(allFiles[i])}, Path: {relativePath}");
                }
                //האם למחוק שורות ריקות על ידי סריקת השורות של כל קובץ ובדיקה עבור כל שורה ושורה האם היא ריקה
                    if (remove)
                    {
                        foreach (var line in lines)
                        {
                            // בדיקה אם השורה אינה ריקה או מכילה רק רווחים
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                writer.WriteLine(line); // כתיבת השורה לקובץ ה-bundle
                            }
                        }
                    }
                    else
                    {
                        foreach (var line in lines)
                        {
                           
                                writer.WriteLine(line); // כתיבת השורה לקובץ ה-bundle
                        }
                    }
                    
                }

              
            }

    }
},bundleoption, languageoption, noteoption,sortoption,empty_linesoption,authorOption);

var create_rsp = new Command("create_rsp", "Creating a response file with a ready command");

create_rsp.SetHandler(async () =>
{
    // קלט עבור השפות
    Console.Write("Enter languages (comma separated or 'all'): ");
    var languagesInput = Console.ReadLine();
    var validLanguages = new HashSet<string> { "cs", "py", "js", "html", "css", "all" };
    // פיצול הקלט ובדיקת תקינות
    var languages = languagesInput.Split(',').Select(lang => lang.Trim().ToLower()).ToList();
    // בדיקה אם כל המילים שהוזנו הן תקינות
    if (!languages.All(lang => validLanguages.Contains(lang)))
    {
        Console.WriteLine("Invalid language input. Please enter only the following options: 'cs', 'py', 'js', 'html', 'css', or 'all'.");
        return; // יציאה מהפונקציה אם הקלט לא תקין
    }


    // קלט עבור נתיב הפלט
    Console.Write("Enter output file path: ");
    var outputFilePath = Console.ReadLine();
    // בדיקה אם התיקייה בנתיב קיימת
    var outputDirectory = Path.GetDirectoryName(outputFilePath);
    if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
    {
        Console.WriteLine("Invalid output path. The specified directory does not exist. Please enter a valid path.");
        
    }


    // קלט עבור אם לרשום הערות מקור
    Console.Write("Do you want to note the source? (true/false): ");
    var noteInput = Console.ReadLine();
    bool isValidNoteInput = bool.TryParse(noteInput, out bool note);
    if (!isValidNoteInput)
    {
        Console.WriteLine("Invalid input for noting the source. Please enter 'true' or 'false'.");
        return; // יציאה מהפונקציה במקרה של קלט לא תקין
    }


    // קלט עבור מיון
    Console.Write("Sort by (name/language): ");
    var sort = Console.ReadLine();
    if (sort != "name" && sort != "language")
    {
        Console.WriteLine("Invalid input for sorting. Please enter 'name' or 'language'.");
        return; // יציאה מהפונקציה במקרה של קלט לא תקין
    }


    // קלט עבור אם למחוק שורות ריקות
    Console.Write("Do you want to remove empty lines? (true/false): ");
    var removeInput = Console.ReadLine();
    bool isValidRemoveInput = bool.TryParse(removeInput, out bool remove);
    if (!isValidRemoveInput)
    {
        Console.WriteLine("Invalid input for removing empty lines. Please enter 'true' or 'false'.");
        return; // יציאה מהפונקציה במקרה של קלט לא תקין
    }


    // קלט עבור שם היוצר
    Console.Write("Enter author name: ");
    var author = Console.ReadLine();

    // בנה פקודת bundle
    string command = $"fib bundle --language {languagesInput} --output {outputFilePath} " +
                     $"{(note ? "--note " : "")}" +
                     $"--sort {sort} " +
                     $"{(remove ? "--remove " : "")}" +
                     $"{(string.IsNullOrWhiteSpace(author) ? "" : $"--author \"{author}\"")}".Trim();

    // שמירה לקובץ תגובה
    string responseFilePath = "response.rsp"; 
    await File.WriteAllTextAsync(responseFilePath, command);

    Console.WriteLine($"Response file created: {responseFilePath}");
});

// הוספת הפקודה ל-root command

var rootcomand = new RootCommand("root comand for file bundler cli");
rootcomand.AddCommand(bundlecomand);
rootcomand.AddCommand(create_rsp);
await rootcomand.InvokeAsync(args);

