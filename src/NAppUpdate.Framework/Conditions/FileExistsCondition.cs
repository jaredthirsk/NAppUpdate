﻿using System;
using System.Collections.Generic;
using System.IO;
using AppUpdate.Common;

namespace AppUpdate.Conditions
{
	[Serializable]
    [UpdateConditionAlias("exists")]
    public class FileExistsCondition : IUpdateCondition
    {
        [NauField("localPath",
            "The local path of the file to check. If not set but set under a FileUpdateTask, the LocalPath of the task will be used. Otherwise this condition will be ignored."
            , false)]
        public string LocalPath { get; set; }

        #region IUpdateCondition Members

        public IDictionary<string, string> Attributes { get; private set; }

        public bool IsMet(Tasks.IUpdateTask task)
        {
            string localPath = !string.IsNullOrEmpty(LocalPath) ? LocalPath : Utils.Reflection.GetNauAttribute(task, "LocalPath") as string;
            if (string.IsNullOrEmpty(localPath))
                return false;

            return File.Exists(localPath);
        }

        #endregion
    }
}
