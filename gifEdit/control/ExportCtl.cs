using APNGLib;
using FreeImageAPI;
using gifEdit.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace gifEdit.control {
	public class ExportCtl {
		//public Func<ImageModel> getImageModel = null;
		public Func<int, bool, ImageModel> getImageModelByFrame = null;
		public Action<int, int> onUpdateProgress = null;

		public Bitmap toBitMap(ImageModel md) {
			Bitmap pic = new Bitmap(md.width, md.height, PixelFormat.Format32bppArgb);

			for(int x = 0; x < md.width; x++) {
				for(int y = 0; y < md.height; y++) {
					int idx = (y * md.width + x) * md.pxChannel;
					Color c = Color.FromArgb(
						md.data[idx + 3],
						md.data[idx + 2],
						md.data[idx + 1],
						md.data[idx + 0]
					);
					pic.SetPixel(x, md.height - 1 - y, c);
				}
			}

			return pic;
		}

		public void saveToClipboard() {
			ImageModel md = getImageModelByFrame(-1, true);
			if(md == null) {
				return;
			}

			Bitmap pic = toBitMap(md);
			ClipboardImageCtl.SetClipboardImage(pic, null, null);
		}

		public void saveAsPng(string savePath, int frameIdx) {
			ImageModel md = getImageModelByFrame(frameIdx, true);
			if(md == null) {
				return;
			}

			FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
			IntPtr pFibData = FreeImage.GetBits(dib);
			Marshal.Copy(md.data, 0, pFibData, md.data.Length);

			FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dib, savePath, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
			//FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, ms, FREE_IMAGE_SAVE_FLAGS.DEFAULT);

			//int size = FreeImage.TellMemory(ms);
			//byte[] buffer = new byte[size];
			//FreeImage.SeekMemory(ms, 0, SeekOrigin.Begin);
			//FreeImage.ReadMemory(buffer, (uint)size, (uint)size, ms);

			FreeImage.Unload(dib);
		}

		public void saveAsGif(string savePath, int startFrameIdx, int frameCount, int frameTime) {
			//ImageModel md = getImageModelByFrame(startFrameIdx);
			//if(md == null) {
			//	return;
			//}

			//FIMEMORY ms = new FIMEMORY();
			//FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
			//IntPtr pFibData = FreeImage.GetBits(dib);
			//Marshal.Copy(md.data, 0, pFibData, md.data.Length);
			//FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, ms, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
			//FreeImage.SeekMemory(ms, 0, SeekOrigin.Begin);

			//FreeImage.LoadMultiBitmapFromMemory(FREE_IMAGE_FORMAT.FIF_GIF, ms, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
			//FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);

			try {
				//freeimage gif---
				//FIMULTIBITMAP mdib = FreeImage.OpenMultiBitmap(FREE_IMAGE_FORMAT.FIF_GIF, savePath, true, false, false, FREE_IMAGE_LOAD_FLAGS.DEFAULT);

				//for(int i = startFrameIdx; i < frameCount; ++i) {
				//	ImageModel md = getImageModelByFrame(i);
				//	if(md == null) {
				//		continue;
				//	}
				//	FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
				//	IntPtr pFibData = FreeImage.GetBits(dib);
				//	Marshal.Copy(md.data, 0, pFibData, md.data.Length);

				//	FITAG tag = FreeImage.CreateTag();
				//	//Debug.WriteLine("aaa:" + FreeImage.GetTagKey(tag));
				//	FreeImage.SetMetadata(FREE_IMAGE_MDMODEL.FIMD_ANIMATION, dib, FreeImage.GetTagKey(tag), tag);

				//	if(tag != null) {
				//		FreeImage.SetTagKey(tag, "FrameTime");
				//		FreeImage.SetTagType(tag, FREE_IMAGE_MDTYPE.FIDT_LONG);
				//		FreeImage.SetTagCount(tag, 1);
				//		FreeImage.SetTagLength(tag, 4);
				//		FreeImage.SetTagValue(tag, BitConverter.GetBytes(frameTime));
				//		FreeImage.SetMetadata(FREE_IMAGE_MDMODEL.FIMD_ANIMATION, dib, FreeImage.GetTagKey(tag), tag);
				//		FreeImage.DeleteTag(tag);
				//	}

				//	FreeImage.AppendPage(mdib, dib);

				//	//FreeImage.Unload(dib);
				//}

				//Debug.WriteLine("bbb:" + FreeImage.GetPageCount(mdib));
				//FreeImage.CloseMultiBitmap(mdib, FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION);

				//AnimatedGif---
				using(var gif = AnimatedGif.AnimatedGif.Create(savePath, frameTime)) {
					for(int i = startFrameIdx; i < frameCount; ++i) {
						ImageModel md = getImageModelByFrame(i, false);
						if(md == null) {
							continue;
						}

						Bitmap pic = toBitMap(md);
						gif.AddFrame(pic, delay: -1, quality: AnimatedGif.GifQuality.Bit8);
					}
				}

			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void saveAsAPNG(string savePath, int startFrameIdx, int frameCount, int fps) {
			if(isThreakWork()) {
				return;
			}

			initThread(savePath, startFrameIdx, frameCount, fps);
			return;

			//try {
			//	if(frameCount <= 0) {
			//		return;
			//	}
				
			//	APNG apngBase = new APNG();
			//	for(int i = 0; i < frameCount; ++i) {
			//		ImageModel md = getImageModelByFrame(startFrameIdx + i, false);
			//		if(md == null) {
			//			return;
			//		}

			//		FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
			//		IntPtr pFibData = FreeImage.GetBits(dib);
			//		Marshal.Copy(md.data, 0, pFibData, md.data.Length);

			//		//FIMEMORY fm = new FIMEMORY();
			//		FIMEMORY fm = FreeImage.OpenMemory(IntPtr.Zero, 0);
			//		FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, fm, FREE_IMAGE_SAVE_FLAGS.DEFAULT);

			//		FreeImage.SeekMemory(fm, 0, SeekOrigin.End);
			//		int bufferSize = FreeImage.TellMemory(fm);
			//		//Debug.WriteLine("aaa:" + bufferSize);

			//		FreeImage.SeekMemory(fm, 0, SeekOrigin.Begin);
			//		byte[] buffer = new byte[bufferSize];
			//		FreeImage.ReadMemory(buffer, (uint)bufferSize, (uint)bufferSize, fm);

			//		FreeImage.CloseMemory(fm);
			//		FreeImage.Unload(dib);

			//		//using(FileStream fs = new FileStream("aaa_" + i + ".png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)) {
			//		//	fs.Write(buffer, 0, bufferSize);
			//		//}

			//		//using(FileStream fs = new FileStream("aaa.png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)) {
			//		//	fs.Write(buffer, 0, bufferSize);
			//		//}

			//		PNG pngFrame = new PNG();
			//		using(MemoryStream ms = new MemoryStream(buffer)) {
			//			pngFrame.Load(ms);
			//		}

			//		apngBase.Add(pngFrame, fps);
			//	}

			//	apngBase.Save(savePath);

			//} catch(Exception ex) {
			//	Debug.WriteLine(ex.ToString());
			//}
		}

		ThreadManageModel mdThreadManage = new ThreadManageModel();
		ThreadCollectModel mdThreadCollect = new ThreadCollectModel();
		List<ThreadWorkModel> lstThreadWork = new List<ThreadWorkModel>();
		WaitAbortThread watiAbortThread = new WaitAbortThread();

		bool isThreakWork() {
			return lstThreadWork.Count > 0;
		}

		//Thread thManage = null;
		//Thread thCollect = null;
		//bool isThreadStop = false;
		void initThread(string savePath, int startFrameIdx, int frameCount, int fps) {
			clearThread();

			nowProgress = 0;
			totalProgress = frameCount + 1;

			mdThreadManage = new ThreadManageModel();
			mdThreadManage.startFrameIdx = startFrameIdx;
			mdThreadManage.frameCount = frameCount;
			mdThreadManage.fps = fps;
			mdThreadManage.isTransparency = true;
			mdThreadManage.th = new Thread(manageProc);
			mdThreadManage.th.Priority = ThreadPriority.BelowNormal;
			mdThreadManage.th.Start(mdThreadManage);

			mdThreadCollect = new ThreadCollectModel();
			mdThreadCollect.th = new Thread(collectProc);
			mdThreadCollect.frameCount = frameCount;
			mdThreadCollect.fps = fps;
			mdThreadCollect.savePath = savePath;
			mdThreadCollect.th.Priority = ThreadPriority.BelowNormal;
			mdThreadCollect.th.Start(mdThreadCollect);

			int count = Environment.ProcessorCount - 1;
			count = Math.Max(1, count);
			for(int i = 0; i < count; ++i) {
				ThreadWorkModel md = new ThreadWorkModel();
				md.idx = i;
				md.th = new Thread(workProc);
				md.th.Priority = ThreadPriority.BelowNormal;
				lstThreadWork.Add(md);
				md.th.Start(md);
			}
		}

		bool isThreadWork() {
			return false;
		}

		public void clearThread() {
			const int waitTime = 300;

			mdThreadManage.needStop = true;
			watiAbortThread.add(mdThreadManage.th, waitTime);
			mdThreadCollect.needStop = true;
			watiAbortThread.add(mdThreadCollect.th, waitTime);

			for(int i = 0; i < lstThreadWork.Count; ++i) {
				lstThreadWork[i].needStop = true;
				watiAbortThread.add(lstThreadWork[i].th, waitTime);
			}
			lstThreadWork = new List<ThreadWorkModel>();
		}

		void manageProc(object data) {
			ThreadManageModel md = data as ThreadManageModel;
			//if(mdThreadManage == null) {
			//	return;
			//}
			
			Window win = MainModel.ins.mainWin;

			for(int i = 0; i < md.frameCount; ++i) {
				ImageModel imgMd = null;
				win.Dispatcher.Invoke(() => {
					imgMd = getImageModelByFrame(md.startFrameIdx + i, md.isTransparency);
				});

				if(md.needStop) {
					return;
				}

				if(imgMd == null) {
					clearThread();
					return;
				}

				int idx = findFreeThreadIdx();
				addWorkTask(lstThreadWork[idx], i, imgMd);
			}
		}

		int findFreeThreadIdx() {
			int minCount = int.MaxValue;
			int minIdx = -1;
			for(int i = 0; i < lstThreadWork.Count; ++i) {
				//if(lstThreadWork[i].queImg.Count < minCount) {
				//	minCount = lstThreadWork[i].queImg.Count;
				//	minIdx = i;
				//}
				if(lstThreadWork[i].mapData.Count < minCount) {
					minCount = lstThreadWork[i].mapData.Count;
					minIdx = i;
				}
			}

			return minIdx;
		}

		void addWorkTask(ThreadWorkModel md, int idx, ImageModel imgMd) {
			lock(md.lockCom) {
				//md.queImg.Enqueue(img);
				md.mapData[idx] = imgMd;
			}
		}

		void removeFirstWorkTask(ThreadWorkModel md, int idx) {
			lock(md.lockCom) {
				md.mapData.Remove(idx);
			}
		}

		void collectProc(object data) {
			ThreadCollectModel md = data as ThreadCollectModel;

			APNG apngBase = new APNG();

			do {
				if(md.needStop) {
					return;
				}

				const int waitTime = 10;

				//Dictionary<int, PNG> mapData = new Dictionary<int, PNG>();

				List<PNG> lstData = new List<PNG>();
				lock(md.mapData) {
					for(int i = 0; i < md.frameCount; ++i) {
						int idx = md.nowFrameIdx + i;
						if(!md.mapData.ContainsKey(idx)) {
							break;
						}

						lstData.Add(md.mapData[idx]);
						md.mapData.Remove(idx);
					}
				}

				if(lstData.Count <= 0) {
					Thread.Sleep(waitTime);
					continue;
				}

				for(int i = 0; i < lstData.Count; ++i) {
					apngBase.Add(lstData[i], md.fps);
					updateProgress();
				}

				md.nowFrameIdx += lstData.Count;
				if(md.nowFrameIdx >= md.frameCount) {
					apngBase.Save(md.savePath);
					updateProgressToEnd();
					clearThread();
				}
			} while(!md.needStop);
		}

		void workProc(object data) {
			ThreadWorkModel md = data as ThreadWorkModel;
			ThreadCollectModel thCollect = mdThreadCollect;

			do {
				if(md.needStop) {
					return;
				}

				const int waitTime = 10;

				Dictionary<int, ImageModel> mapData = new Dictionary<int, ImageModel>();
				lock(md.lockCom) {
					foreach(int key in md.mapData.Keys) {
						mapData[key] = md.mapData[key];
					}

					md.mapData = new Dictionary<int, ImageModel>();
				}

				if(mapData.Count <= 0) {
					Thread.Sleep(waitTime);
					continue;
				}

				foreach(int key in mapData.Keys) {
					PNG png = formatPng(mapData[key]);

					lock(thCollect.lockCom) {
						thCollect.mapData[key] = png;
					}
				}
			} while(!md.needStop);
		}

		int nowProgress = 0;
		int totalProgress = 0;
		object lockProgress = new object();
		void updateProgress(int count = 1) {
			int val = 0;
			lock(lockProgress) {
				nowProgress += count;
				val = nowProgress;
			}

			onUpdateProgress?.Invoke(val, totalProgress);
		}

		void updateProgressToEnd() {
			int val = 0;
			lock(lockProgress) {
				nowProgress = totalProgress;
				val = nowProgress;
			}

			onUpdateProgress?.Invoke(val, totalProgress);
		}

		private PNG formatPng(ImageModel imgMd) {
			try {
				FIBITMAP dib = FreeImage.Allocate(imgMd.width, imgMd.height, 32, 8, 8, 8);
				IntPtr pFibData = FreeImage.GetBits(dib);
				Marshal.Copy(imgMd.data, 0, pFibData, imgMd.data.Length);

				//FIMEMORY fm = new FIMEMORY();
				FIMEMORY fm = FreeImage.OpenMemory(IntPtr.Zero, 0);
				FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, fm, FREE_IMAGE_SAVE_FLAGS.DEFAULT);

				FreeImage.SeekMemory(fm, 0, SeekOrigin.End);
				int bufferSize = FreeImage.TellMemory(fm);
				//Debug.WriteLine("aaa:" + bufferSize);

				FreeImage.SeekMemory(fm, 0, SeekOrigin.Begin);
				byte[] buffer = new byte[bufferSize];
				FreeImage.ReadMemory(buffer, (uint)bufferSize, (uint)bufferSize, fm);

				FreeImage.CloseMemory(fm);
				FreeImage.Unload(dib);

				//using(FileStream fs = new FileStream("aaa_" + i + ".png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)) {
				//	fs.Write(buffer, 0, bufferSize);
				//}

				//using(FileStream fs = new FileStream("aaa.png", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)) {
				//	fs.Write(buffer, 0, bufferSize);
				//}

				PNG pngFrame = new PNG();
				using(MemoryStream ms = new MemoryStream(buffer)) {
					pngFrame.Load(ms);
				}

				return pngFrame;
			} catch(Exception) { }

			return null;
		}

		class ThreadManageModel {
			public Thread th = null;
			public bool needStop = false;

			public int startFrameIdx = 0;
			public int frameCount = 0;
			public int fps = 0;
			public bool isTransparency = false;
		}

		class ThreadWorkModel {
			public Thread th = null;
			public bool needStop = false;

			public int idx;
			//public Queue<ImageModel> queImg;
			public Dictionary<int, ImageModel> mapData = new Dictionary<int, ImageModel>();

			public object lockCom = new object();
		}

		class ThreadCollectModel {
			public Thread th = null;
			public bool needStop = false;

			public int nowFrameIdx = 0;
			public int frameCount = 0;
			public int fps = 0;
			public string savePath = "";

			public object lockCom = new object();
			public Dictionary<int, PNG> mapData = new Dictionary<int, PNG>();
		}

		class WaitAbortThread {
			public Thread th = null;

			List<AbortThreadModel> lstTh = new List<AbortThreadModel>();
			object lockList = new object();

			public void add(Thread th, int watiTime) {
				if(th == null || !th.IsAlive) {
					return;
				}

				lock(lockList) {
					if(lstTh.Count <= 0) {
						th = new Thread(proc);
						th.Priority = ThreadPriority.BelowNormal;
						th.Start();
					}
				}
			}

			void proc() {
				const int sleepTime = 10;
				bool isStop = false;
				do {
					if(lstTh.Count <= 0) {
						Thread.Sleep(sleepTime);
						continue;
					}

					lock(lockList) {
						List<int> lstRemoveIdx = new List<int>();

						for(int i = 0; i < lstTh.Count; ++i) {
							lstTh[i].nowTime += sleepTime;

							if(!lstTh[i].th.IsAlive) {
								lstRemoveIdx.Add(i);
								continue;
							}

							if(lstTh[i].nowTime >= lstTh[i].watiTime) {
								try {
									lstTh[i].th.Abort();
								} catch(Exception) { }
								lstRemoveIdx.Add(i);
							}
						}

						for(int i = lstRemoveIdx.Count - 1; i >= 0; --i) {
							lstTh.RemoveAt(i);
						}

						if(lstTh.Count <= 0) {
							isStop = true;
						}
					}

					if(!isStop) {
						Thread.Sleep(sleepTime);
					}
				} while(isStop);
			}

			public void clear() {
				try {
					lock(lockList) {
						for(int i = 0; i < lstTh.Count; ++i) {
							try {
								lstTh[i].th.Abort();
							} catch(Exception) { }
						}
						lstTh.Clear();
					}
					th?.Abort();
				} catch(Exception) {

				}
				th = null;
			}
		}

		class AbortThreadModel {
			public Thread th = null;
			public int watiTime = 0;
			public int nowTime = 0;

			public AbortThreadModel() {

			}

			public AbortThreadModel(Thread _th, int _watiTime) {
				th = _th;
				watiTime = _watiTime;
			}
		}
	}
}
