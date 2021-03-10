using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dotnetcnblog.TagHandlers;
using Dotnetcnblog.Utils;
using McMaster.Extensions.CommandLineUtils;
using Console = Colorful.Console;

namespace Dotnetcnblog.Command
{
    [Command(Name = "upload", Description = "上传单个图片")]
    public class CommandUploadImg : ICommand
    {
        private static readonly Dictionary<string, string> ReplaceDic = new Dictionary<string, string>();

        [Required]
        [Argument(0)]
        public string[] FilePaths { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            if (app.Options.Count == 1 && app.Options[0].ShortName == "h")
            {
                app.ShowHelp();
            }

            Execute(CommandContextStore.Get());
            return 0;
        }

        public void Execute(CommandContext context)
        {
            try
            {
                var tasks = new List<Task<string>>();
                foreach (var filePath in FilePaths)
                {
                    // 使用多线程上传
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            if (!File.Exists(filePath))
                            {
                                ConsoleHelper.PrintError($"文件不存在：{filePath}");
                                return null;
                            }
                            else
                            {
                                var imgUrl = ImageUploadHelper.Upload(filePath);
                                return imgUrl;
                            }
                        }
                        catch
                        {
                            return "";
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                foreach (var task in tasks)
                {
                    ConsoleHelper.PrintMsg($"{task.Result}");
                }
            }
            catch (Exception e)
            {
                ConsoleHelper.PrintError(e.Message);
            }
        }
    }
}