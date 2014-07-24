using log4net.Core;
using log4net.Util;
using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace log4net.Appender
{
	public class RollingFileAppender : FileAppender
	{
		private readonly static Type declaringType;

		private RollingFileAppender.IDateTime m_dateTime;

		private string m_datePattern = ".yyyy-MM-dd";

		private string m_scheduledFilename;

		private DateTime m_nextCheck = DateTime.MaxValue;

		private DateTime m_now;

		private RollingFileAppender.RollPoint m_rollPoint;

		private long m_maxFileSize = (long)10485760;

		private int m_maxSizeRollBackups;

		private int m_curSizeRollBackups;

		private int m_countDirection = -1;

		private RollingFileAppender.RollingMode m_rollingStyle = RollingFileAppender.RollingMode.Composite;

		private bool m_rollDate = true;

		private bool m_rollSize = true;

		private bool m_staticLogFileName = true;

		private bool m_preserveLogFileNameExtension;

		private string m_baseFileName;

		private readonly static DateTime s_date1970;

		public int CountDirection
		{
			get
			{
				return this.m_countDirection;
			}
			set
			{
				this.m_countDirection = value;
			}
		}

		public string DatePattern
		{
			get
			{
				return this.m_datePattern;
			}
			set
			{
				this.m_datePattern = value;
			}
		}

		public RollingFileAppender.IDateTime DateTimeStrategy
		{
			get
			{
				return this.m_dateTime;
			}
			set
			{
				this.m_dateTime = value;
			}
		}

		public long MaxFileSize
		{
			get
			{
				return this.m_maxFileSize;
			}
			set
			{
				this.m_maxFileSize = value;
			}
		}

		public string MaximumFileSize
		{
			get
			{
				return this.m_maxFileSize.ToString(NumberFormatInfo.InvariantInfo);
			}
			set
			{
				this.m_maxFileSize = OptionConverter.ToFileSize(value, this.m_maxFileSize + (long)1);
			}
		}

		public int MaxSizeRollBackups
		{
			get
			{
				return this.m_maxSizeRollBackups;
			}
			set
			{
				this.m_maxSizeRollBackups = value;
			}
		}

		public bool PreserveLogFileNameExtension
		{
			get
			{
				return this.m_preserveLogFileNameExtension;
			}
			set
			{
				this.m_preserveLogFileNameExtension = value;
			}
		}

		public RollingFileAppender.RollingMode RollingStyle
		{
			get
			{
				return this.m_rollingStyle;
			}
			set
			{
				this.m_rollingStyle = value;
				switch (this.m_rollingStyle)
				{
					case RollingFileAppender.RollingMode.Once:
					{
						this.m_rollDate = false;
						this.m_rollSize = false;
						base.AppendToFile = false;
						return;
					}
					case RollingFileAppender.RollingMode.Size:
					{
						this.m_rollDate = false;
						this.m_rollSize = true;
						return;
					}
					case RollingFileAppender.RollingMode.Date:
					{
						this.m_rollDate = true;
						this.m_rollSize = false;
						return;
					}
					case RollingFileAppender.RollingMode.Composite:
					{
						this.m_rollDate = true;
						this.m_rollSize = true;
						return;
					}
					default:
					{
						return;
					}
				}
			}
		}

		public bool StaticLogFileName
		{
			get
			{
				return this.m_staticLogFileName;
			}
			set
			{
				this.m_staticLogFileName = value;
			}
		}

		static RollingFileAppender()
		{
			RollingFileAppender.declaringType = typeof(RollingFileAppender);
			RollingFileAppender.s_date1970 = new DateTime(1970, 1, 1);
		}

		public RollingFileAppender()
		{
		}

		public override void ActivateOptions()
		{
			if (this.m_dateTime == null)
			{
				this.m_dateTime = new RollingFileAppender.LocalDateTime();
			}
			if (this.m_rollDate && this.m_datePattern != null)
			{
				this.m_now = this.m_dateTime.Now;
				this.m_rollPoint = this.ComputeCheckPeriod(this.m_datePattern);
				if (this.m_rollPoint == RollingFileAppender.RollPoint.InvalidRollPoint)
				{
					throw new ArgumentException(string.Concat("Invalid RollPoint, unable to parse [", this.m_datePattern, "]"));
				}
				this.m_nextCheck = this.NextCheckDate(this.m_now, this.m_rollPoint);
			}
			else if (this.m_rollDate)
			{
				this.ErrorHandler.Error(string.Concat("Either DatePattern or rollingStyle options are not set for [", base.Name, "]."));
			}
			if (base.SecurityContext == null)
			{
				base.SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
			}
			using (IDisposable disposable = base.SecurityContext.Impersonate(this))
			{
				base.File = FileAppender.ConvertToFullPath(base.File.Trim());
				this.m_baseFileName = base.File;
			}
			if (this.m_rollDate && this.File != null && this.m_scheduledFilename == null)
			{
				this.m_scheduledFilename = this.CombinePath(this.File, this.m_now.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo));
			}
			this.ExistingInit();
			base.ActivateOptions();
		}

		protected virtual void AdjustFileBeforeAppend()
		{
			if (this.m_rollDate)
			{
				DateTime now = this.m_dateTime.Now;
				if (now >= this.m_nextCheck)
				{
					this.m_now = now;
					this.m_nextCheck = this.NextCheckDate(this.m_now, this.m_rollPoint);
					this.RollOverTime(true);
				}
			}
			if (this.m_rollSize && this.File != null && ((CountingQuietTextWriter)base.QuietWriter).Count >= this.m_maxFileSize)
			{
				this.RollOverSize();
			}
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			this.AdjustFileBeforeAppend();
			base.Append(loggingEvent);
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			this.AdjustFileBeforeAppend();
			base.Append(loggingEvents);
		}

		private string CombinePath(string path1, string path2)
		{
			string extension = Path.GetExtension(path1);
			if (!this.m_preserveLogFileNameExtension || extension.Length <= 0)
			{
				return string.Concat(path1, path2);
			}
			return Path.Combine(Path.GetDirectoryName(path1), string.Concat(Path.GetFileNameWithoutExtension(path1), path2, extension));
		}

		private RollingFileAppender.RollPoint ComputeCheckPeriod(string datePattern)
		{
			string str = RollingFileAppender.s_date1970.ToString(datePattern, DateTimeFormatInfo.InvariantInfo);
			for (int i = 0; i <= 5; i++)
			{
				DateTime dateTime = this.NextCheckDate(RollingFileAppender.s_date1970, (RollingFileAppender.RollPoint)i);
				string str1 = dateTime.ToString(datePattern, DateTimeFormatInfo.InvariantInfo);
				Type type = RollingFileAppender.declaringType;
				object[] objArray = new object[] { "Type = [", i, "], r0 = [", str, "], r1 = [", str1, "]" };
				LogLog.Debug(type, string.Concat(objArray));
				if (str != null && str1 != null && !str.Equals(str1))
				{
					return (RollingFileAppender.RollPoint)i;
				}
			}
			return RollingFileAppender.RollPoint.InvalidRollPoint;
		}

		protected void DeleteFile(string fileName)
		{
			if (this.FileExists(fileName))
			{
				string str = fileName;
				object[] objArray = new object[] { fileName, ".", Environment.TickCount, ".DeletePending" };
				string str1 = string.Concat(objArray);
				try
				{
					using (IDisposable disposable = base.SecurityContext.Impersonate(this))
					{
						System.IO.File.Move(fileName, str1);
					}
					str = str1;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Type type = RollingFileAppender.declaringType;
					string[] strArrays = new string[] { "Exception while moving file to be deleted [", fileName, "] -> [", str1, "]" };
					LogLog.Debug(type, string.Concat(strArrays), exception);
				}
				try
				{
					using (IDisposable disposable1 = base.SecurityContext.Impersonate(this))
					{
						System.IO.File.Delete(str);
					}
					LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Deleted file [", fileName, "]"));
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					if (str != fileName)
					{
						LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Exception while deleting temp file [", str, "]"), exception2);
					}
					else
					{
						this.ErrorHandler.Error(string.Concat("Exception while deleting file [", str, "]"), exception2, ErrorCode.GenericFailure);
					}
				}
			}
		}

		private void DetermineCurSizeRollBackups()
		{
			this.m_curSizeRollBackups = 0;
			string fullPath = null;
			string fileName = null;
			using (IDisposable disposable = base.SecurityContext.Impersonate(this))
			{
				fullPath = Path.GetFullPath(this.m_baseFileName);
				fileName = Path.GetFileName(fullPath);
			}
			this.InitializeRollBackups(fileName, this.GetExistingFiles(fullPath));
			LogLog.Debug(RollingFileAppender.declaringType, string.Concat("curSizeRollBackups starts at [", this.m_curSizeRollBackups, "]"));
		}

		protected void ExistingInit()
		{
			this.DetermineCurSizeRollBackups();
			this.RollOverIfDateBoundaryCrossing();
			if (!base.AppendToFile)
			{
				bool flag = false;
				string nextOutputFileName = this.GetNextOutputFileName(this.m_baseFileName);
				using (IDisposable disposable = base.SecurityContext.Impersonate(this))
				{
					flag = System.IO.File.Exists(nextOutputFileName);
				}
				if (flag)
				{
					if (this.m_maxSizeRollBackups == 0)
					{
						LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Output file [", nextOutputFileName, "] already exists. MaxSizeRollBackups is 0; cannot roll. Overwriting existing file."));
						return;
					}
					LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Output file [", nextOutputFileName, "] already exists. Not appending to file. Rolling existing file out of the way."));
					this.RollOverRenameFiles(nextOutputFileName);
				}
			}
		}

		protected bool FileExists(string path)
		{
			bool flag;
			using (IDisposable disposable = base.SecurityContext.Impersonate(this))
			{
				flag = System.IO.File.Exists(path);
			}
			return flag;
		}

		private int GetBackUpIndex(string curFileName)
		{
			int num = -1;
			string fileNameWithoutExtension = curFileName;
			if (this.m_preserveLogFileNameExtension)
			{
				fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithoutExtension);
			}
			int num1 = fileNameWithoutExtension.LastIndexOf(".");
			if (num1 > 0)
			{
				SystemInfo.TryParse(fileNameWithoutExtension.Substring(num1 + 1), out num);
			}
			return num;
		}

		private ArrayList GetExistingFiles(string baseFilePath)
		{
			ArrayList arrayLists = new ArrayList();
			string directoryName = null;
			using (IDisposable disposable = base.SecurityContext.Impersonate(this))
			{
				string fullPath = Path.GetFullPath(baseFilePath);
				directoryName = Path.GetDirectoryName(fullPath);
				if (Directory.Exists(directoryName))
				{
					string fileName = Path.GetFileName(fullPath);
					string[] files = Directory.GetFiles(directoryName, this.GetWildcardPatternForFile(fileName));
					if (files != null)
					{
						for (int i = 0; i < (int)files.Length; i++)
						{
							string str = Path.GetFileName(files[i]);
							if (str.StartsWith(Path.GetFileNameWithoutExtension(fileName)))
							{
								arrayLists.Add(str);
							}
						}
					}
				}
			}
			LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Searched for existing files in [", directoryName, "]"));
			return arrayLists;
		}

		protected string GetNextOutputFileName(string fileName)
		{
			if (!this.m_staticLogFileName)
			{
				fileName = fileName.Trim();
				if (this.m_rollDate)
				{
					fileName = this.CombinePath(fileName, this.m_now.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo));
				}
				if (this.m_countDirection >= 0)
				{
					fileName = this.CombinePath(fileName, string.Concat(".", this.m_curSizeRollBackups));
				}
			}
			return fileName;
		}

		private string GetWildcardPatternForFile(string baseFileName)
		{
			if (!this.m_preserveLogFileNameExtension)
			{
				return string.Concat(baseFileName, '*');
			}
			return string.Concat(Path.GetFileNameWithoutExtension(baseFileName), ".*", Path.GetExtension(baseFileName));
		}

		private void InitializeFromOneFile(string baseFile, string curFileName)
		{
			if (!curFileName.StartsWith(Path.GetFileNameWithoutExtension(baseFile)))
			{
				return;
			}
			if (curFileName.Equals(baseFile))
			{
				return;
			}
			if (this.m_rollDate && !this.m_staticLogFileName && !curFileName.StartsWith(this.CombinePath(baseFile, this.m_dateTime.Now.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo))))
			{
				LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Ignoring file [", curFileName, "] because it is from a different date period"));
				return;
			}
			try
			{
				int backUpIndex = this.GetBackUpIndex(curFileName);
				if (backUpIndex > this.m_curSizeRollBackups)
				{
					if (this.m_maxSizeRollBackups != 0)
					{
						if (-1 == this.m_maxSizeRollBackups)
						{
							this.m_curSizeRollBackups = backUpIndex;
						}
						else if (this.m_countDirection >= 0)
						{
							this.m_curSizeRollBackups = backUpIndex;
						}
						else if (backUpIndex <= this.m_maxSizeRollBackups)
						{
							this.m_curSizeRollBackups = backUpIndex;
						}
					}
					Type type = RollingFileAppender.declaringType;
					object[] objArray = new object[] { "File name [", curFileName, "] moves current count to [", this.m_curSizeRollBackups, "]" };
					LogLog.Debug(type, string.Concat(objArray));
				}
			}
			catch (FormatException formatException)
			{
				LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Encountered a backup file not ending in .x [", curFileName, "]"));
			}
		}

		private void InitializeRollBackups(string baseFile, ArrayList arrayFiles)
		{
			if (arrayFiles != null)
			{
				string lower = baseFile.ToLower(CultureInfo.InvariantCulture);
				foreach (string arrayFile in arrayFiles)
				{
					this.InitializeFromOneFile(lower, arrayFile.ToLower(CultureInfo.InvariantCulture));
				}
			}
		}

		protected DateTime NextCheckDate(DateTime currentDateTime, RollingFileAppender.RollPoint rollPoint)
		{
			DateTime dateTime = currentDateTime;
			switch (rollPoint)
			{
				case RollingFileAppender.RollPoint.TopOfMinute:
				{
					dateTime = dateTime.AddMilliseconds((double)(-dateTime.Millisecond));
					dateTime = dateTime.AddSeconds((double)(-dateTime.Second));
					dateTime = dateTime.AddMinutes(1);
					break;
				}
				case RollingFileAppender.RollPoint.TopOfHour:
				{
					dateTime = dateTime.AddMilliseconds((double)(-dateTime.Millisecond));
					dateTime = dateTime.AddSeconds((double)(-dateTime.Second));
					dateTime = dateTime.AddMinutes((double)(-dateTime.Minute));
					dateTime = dateTime.AddHours(1);
					break;
				}
				case RollingFileAppender.RollPoint.HalfDay:
				{
					dateTime = dateTime.AddMilliseconds((double)(-dateTime.Millisecond));
					dateTime = dateTime.AddSeconds((double)(-dateTime.Second));
					dateTime = dateTime.AddMinutes((double)(-dateTime.Minute));
					if (dateTime.Hour >= 12)
					{
						dateTime = dateTime.AddHours((double)(-dateTime.Hour));
						dateTime = dateTime.AddDays(1);
						break;
					}
					else
					{
						dateTime = dateTime.AddHours((double)(12 - dateTime.Hour));
						break;
					}
				}
				case RollingFileAppender.RollPoint.TopOfDay:
				{
					dateTime = dateTime.AddMilliseconds((double)(-dateTime.Millisecond));
					dateTime = dateTime.AddSeconds((double)(-dateTime.Second));
					dateTime = dateTime.AddMinutes((double)(-dateTime.Minute));
					dateTime = dateTime.AddHours((double)(-dateTime.Hour));
					dateTime = dateTime.AddDays(1);
					break;
				}
				case RollingFileAppender.RollPoint.TopOfWeek:
				{
					dateTime = dateTime.AddMilliseconds((double)(-dateTime.Millisecond));
					dateTime = dateTime.AddSeconds((double)(-dateTime.Second));
					dateTime = dateTime.AddMinutes((double)(-dateTime.Minute));
					dateTime = dateTime.AddHours((double)(-dateTime.Hour));
					dateTime = dateTime.AddDays((double)((int)(DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday | DayOfWeek.Saturday) - (int)dateTime.DayOfWeek));
					break;
				}
				case RollingFileAppender.RollPoint.TopOfMonth:
				{
					dateTime = dateTime.AddMilliseconds((double)(-dateTime.Millisecond));
					dateTime = dateTime.AddSeconds((double)(-dateTime.Second));
					dateTime = dateTime.AddMinutes((double)(-dateTime.Minute));
					dateTime = dateTime.AddHours((double)(-dateTime.Hour));
					dateTime = dateTime.AddDays((double)(1 - dateTime.Day));
					dateTime = dateTime.AddMonths(1);
					break;
				}
			}
			return dateTime;
		}

		protected override void OpenFile(string fileName, bool append)
		{
			lock (this)
			{
				fileName = this.GetNextOutputFileName(fileName);
				long length = (long)0;
				if (append)
				{
					using (IDisposable disposable = base.SecurityContext.Impersonate(this))
					{
						if (System.IO.File.Exists(fileName))
						{
							length = (new FileInfo(fileName)).Length;
						}
					}
				}
				else if (LogLog.IsErrorEnabled && this.m_maxSizeRollBackups != 0 && this.FileExists(fileName))
				{
					LogLog.Error(RollingFileAppender.declaringType, string.Concat("RollingFileAppender: INTERNAL ERROR. Append is False but OutputFile [", fileName, "] already exists."));
				}
				if (!this.m_staticLogFileName)
				{
					this.m_scheduledFilename = fileName;
				}
				base.OpenFile(fileName, append);
				((CountingQuietTextWriter)base.QuietWriter).Count = length;
			}
		}

		protected void RollFile(string fromFile, string toFile)
		{
			if (!this.FileExists(fromFile))
			{
				Type type = RollingFileAppender.declaringType;
				string[] strArrays = new string[] { "Cannot RollFile [", fromFile, "] -> [", toFile, "]. Source does not exist" };
				LogLog.Warn(type, string.Concat(strArrays));
			}
			else
			{
				this.DeleteFile(toFile);
				try
				{
					Type type1 = RollingFileAppender.declaringType;
					string[] strArrays1 = new string[] { "Moving [", fromFile, "] -> [", toFile, "]" };
					LogLog.Debug(type1, string.Concat(strArrays1));
					using (IDisposable disposable = base.SecurityContext.Impersonate(this))
					{
						System.IO.File.Move(fromFile, toFile);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					IErrorHandler errorHandler = this.ErrorHandler;
					string[] strArrays2 = new string[] { "Exception while rolling file [", fromFile, "] -> [", toFile, "]" };
					errorHandler.Error(string.Concat(strArrays2), exception, ErrorCode.GenericFailure);
				}
			}
		}

		private void RollOverIfDateBoundaryCrossing()
		{
			DateTime lastWriteTime;
			if (this.m_staticLogFileName && this.m_rollDate && this.FileExists(this.m_baseFileName))
			{
				using (IDisposable disposable = base.SecurityContext.Impersonate(this))
				{
					lastWriteTime = System.IO.File.GetLastWriteTime(this.m_baseFileName);
				}
				Type type = RollingFileAppender.declaringType;
				string[] str = new string[] { "[", lastWriteTime.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo), "] vs. [", this.m_now.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo), "]" };
				LogLog.Debug(type, string.Concat(str));
				if (!lastWriteTime.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo).Equals(this.m_now.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo)))
				{
					this.m_scheduledFilename = string.Concat(this.m_baseFileName, lastWriteTime.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo));
					LogLog.Debug(RollingFileAppender.declaringType, string.Concat("Initial roll over to [", this.m_scheduledFilename, "]"));
					this.RollOverTime(false);
					LogLog.Debug(RollingFileAppender.declaringType, string.Concat("curSizeRollBackups after rollOver at [", this.m_curSizeRollBackups, "]"));
				}
			}
		}

		protected void RollOverRenameFiles(string baseFileName)
		{
			if (this.m_maxSizeRollBackups != 0)
			{
				if (this.m_countDirection < 0)
				{
					if (this.m_curSizeRollBackups == this.m_maxSizeRollBackups)
					{
						this.DeleteFile(this.CombinePath(baseFileName, string.Concat(".", this.m_maxSizeRollBackups)));
						RollingFileAppender mCurSizeRollBackups = this;
						mCurSizeRollBackups.m_curSizeRollBackups = mCurSizeRollBackups.m_curSizeRollBackups - 1;
					}
					for (int i = this.m_curSizeRollBackups; i >= 1; i--)
					{
						this.RollFile(this.CombinePath(baseFileName, string.Concat(".", i)), this.CombinePath(baseFileName, string.Concat(".", i + 1)));
					}
					RollingFileAppender rollingFileAppender = this;
					rollingFileAppender.m_curSizeRollBackups = rollingFileAppender.m_curSizeRollBackups + 1;
					this.RollFile(baseFileName, this.CombinePath(baseFileName, ".1"));
					return;
				}
				if (this.m_curSizeRollBackups >= this.m_maxSizeRollBackups && this.m_maxSizeRollBackups > 0)
				{
					int num = this.m_curSizeRollBackups - this.m_maxSizeRollBackups;
					if (this.m_staticLogFileName)
					{
						num++;
					}
					string str = baseFileName;
					if (!this.m_staticLogFileName)
					{
						int num1 = str.LastIndexOf(".");
						if (num1 >= 0)
						{
							str = str.Substring(0, num1);
						}
					}
					this.DeleteFile(this.CombinePath(str, string.Concat(".", num)));
				}
				if (this.m_staticLogFileName)
				{
					RollingFileAppender mCurSizeRollBackups1 = this;
					mCurSizeRollBackups1.m_curSizeRollBackups = mCurSizeRollBackups1.m_curSizeRollBackups + 1;
					this.RollFile(baseFileName, this.CombinePath(baseFileName, string.Concat(".", this.m_curSizeRollBackups)));
				}
			}
		}

		protected void RollOverSize()
		{
			base.CloseFile();
			LogLog.Debug(RollingFileAppender.declaringType, string.Concat("rolling over count [", ((CountingQuietTextWriter)base.QuietWriter).Count, "]"));
			LogLog.Debug(RollingFileAppender.declaringType, string.Concat("maxSizeRollBackups [", this.m_maxSizeRollBackups, "]"));
			LogLog.Debug(RollingFileAppender.declaringType, string.Concat("curSizeRollBackups [", this.m_curSizeRollBackups, "]"));
			LogLog.Debug(RollingFileAppender.declaringType, string.Concat("countDirection [", this.m_countDirection, "]"));
			this.RollOverRenameFiles(this.File);
			if (!this.m_staticLogFileName && this.m_countDirection >= 0)
			{
				RollingFileAppender mCurSizeRollBackups = this;
				mCurSizeRollBackups.m_curSizeRollBackups = mCurSizeRollBackups.m_curSizeRollBackups + 1;
			}
			this.SafeOpenFile(this.m_baseFileName, false);
		}

		protected void RollOverTime(bool fileIsOpen)
		{
			if (this.m_staticLogFileName)
			{
				if (this.m_datePattern == null)
				{
					this.ErrorHandler.Error("Missing DatePattern option in rollOver().");
					return;
				}
				string str = this.m_now.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo);
				if (this.m_scheduledFilename.Equals(this.CombinePath(this.File, str)))
				{
					this.ErrorHandler.Error(string.Concat("Compare ", this.m_scheduledFilename, " : ", this.CombinePath(this.File, str)));
					return;
				}
				if (fileIsOpen)
				{
					base.CloseFile();
				}
				for (int i = 1; i <= this.m_curSizeRollBackups; i++)
				{
					string str1 = this.CombinePath(this.File, string.Concat(".", i));
					string str2 = this.CombinePath(this.m_scheduledFilename, string.Concat(".", i));
					this.RollFile(str1, str2);
				}
				this.RollFile(this.File, this.m_scheduledFilename);
			}
			this.m_curSizeRollBackups = 0;
			this.m_scheduledFilename = this.CombinePath(this.File, this.m_now.ToString(this.m_datePattern, DateTimeFormatInfo.InvariantInfo));
			if (fileIsOpen)
			{
				this.SafeOpenFile(this.m_baseFileName, false);
			}
		}

		protected override void SetQWForFiles(TextWriter writer)
		{
			base.QuietWriter = new CountingQuietTextWriter(writer, this.ErrorHandler);
		}

		public interface IDateTime
		{
			DateTime Now
			{
				get;
			}
		}

		private class LocalDateTime : RollingFileAppender.IDateTime
		{
			public DateTime Now
			{
				get
				{
					return DateTime.Now;
				}
			}

			public LocalDateTime()
			{
			}
		}

		public enum RollingMode
		{
			Once,
			Size,
			Date,
			Composite
		}

		protected enum RollPoint
		{
			InvalidRollPoint = -1,
			TopOfMinute = 0,
			TopOfHour = 1,
			HalfDay = 2,
			TopOfDay = 3,
			TopOfWeek = 4,
			TopOfMonth = 5
		}

		private class UniversalDateTime : RollingFileAppender.IDateTime
		{
			public DateTime Now
			{
				get
				{
					return DateTime.UtcNow;
				}
			}

			public UniversalDateTime()
			{
			}
		}
	}
}