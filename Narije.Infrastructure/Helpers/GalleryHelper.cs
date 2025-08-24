using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Narije.Infrastructure.Contexts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;

namespace Narije.Infrastructure.Helpers
{
    public static class GalleryHelper
    {
        static void FixedSize(Image imgPhoto, int Width, int Height, out int destWidth, out int destHeight)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            destWidth = (int)(sourceWidth * nPercent);
            destHeight = (int)(sourceHeight * nPercent);

        }

        public static async Task<int> MakeImageParts(string filePath, string path, Gallery gallery)
        {
            if (gallery.SystemFileName.ToLower().Contains("svg"))
                return 0;

            try
            {
                SixLabors.ImageSharp.Image image = Image.Load(filePath);

                string str = string.Format("{0}{1}_favicon{2}", path, gallery.Id, gallery.SystemFileName);
                if (!System.IO.File.Exists(str))
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new Size(30, 30),
                        Compand = true,
                        Mode = ResizeMode.Stretch
                    };
                    var fav = image.Clone(A => A.Resize(resizeOptions));
                    await fav.SaveAsync(str);
                }

                str = string.Format("{0}{1}_thumbnail{2}", path, gallery.Id, gallery.SystemFileName);
                if (!System.IO.File.Exists(str))
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new Size(150, 150),
                        Compand = true,
                        Mode = ResizeMode.Stretch
                    };
                    var thumb = image.Clone(A => A.Resize(resizeOptions));
                    await thumb.SaveAsync(str);
                }

                str = string.Format("{0}{1}_small{2}", path, gallery.Id, gallery.SystemFileName);
                if (!System.IO.File.Exists(str))
                    if ((image.Width >= 300) || (image.Height >= 300))
                    {
                        var resizeOptions = new ResizeOptions
                        {
                            Size = new Size(300, 300),
                            Compand = true,
                            Mode = ResizeMode.Max
                        };
                        var small = image.Clone(A => A.Resize(resizeOptions));
                        await small.SaveAsync(str);
                    }

                str = string.Format("{0}{1}_medium{2}", path, gallery.Id, gallery.SystemFileName);
                if (!System.IO.File.Exists(str))
                    if (image.Width >= 768)
                    {
                        var resizeOptions = new ResizeOptions
                        {
                            Size = new Size(768, 480),
                            Compand = true,
                            Mode = ResizeMode.Max
                        };
                        var medium = image.Clone(A => A.Resize(resizeOptions));
                        await medium.SaveAsync(str);
                    }

                str = string.Format("{0}{1}_large{2}", path, gallery.Id, gallery.SystemFileName);
                if (!System.IO.File.Exists(str))
                    if (image.Width >= 1024)
                    {
                        var resizeOptions = new ResizeOptions
                        {
                            Size = new Size(1024, 768),
                            Compand = true,
                            Mode = ResizeMode.Max
                        };
                        var large = image.Clone(A => A.Resize(resizeOptions));
                        await large.SaveAsync(str);
                    }

            }
            catch (Exception e)
            {
            }

            return 0;

        }

        public static async Task<int> CheckAndGenerate(Gallery gallery)
        {
            var path = /*contentRoot +*/ "/data/";

            string filename = string.Format("{0}{1}_favicon{2}", path, gallery.Id, gallery.SystemFileName);

            if (System.IO.File.Exists(filename))
                return 0;

            string SysFileName = string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);

            var filePath = path + SysFileName;

            await MakeImageParts(filePath, path, gallery);


            return 1;

        }


        public static async Task<int> AddToGallery(NarijeDBContext _NarijeDBContext, string Source, IFormFile file, bool Hidden = false)
        {
            
            var path = "/data/";

            if (file is null)
                return 0;

            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                var gallery = new Gallery()
                {
                    Source = Source,
                    OriginalFileName = file.FileName,
                    SystemFileName = extension,
                    Hidden = Hidden,
                };
                await _NarijeDBContext.Galleries.AddAsync(gallery);
                await _NarijeDBContext.SaveChangesAsync();

           
                string SysFileName = string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);

                var filePath = path + SysFileName;

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                await MakeImageParts(filePath, path, gallery);

                return gallery.Id;

            }

            return 0;

        }

        public static async Task<int?> EditGallery(NarijeDBContext _NarijeDBContext, int? Id, string Source, IFormFile file, bool Hidden = false)
        {
            //var contentRoot = _IConfiguration.GetValue<string>(WebHostDefaults.ContentRootKey);
            var path = "/data/";

            Gallery gallery = null;

            //مقرر شد به هر حال در هنگام ویرایش یک عکس جدید اضافه شود
            //if (Id != null)
            //    gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == Id).AsNoTracking().FirstOrDefaultAsync();

            if (gallery is null)
            {
                gallery = new Gallery()
                {
                    Source = Source,
                    OriginalFileName = "",
                    SystemFileName = "",
                    Hidden = Hidden
                };
            }

            if (file is null)
                return Id;

            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);

                gallery.OriginalFileName = file.FileName;
                gallery.SystemFileName = extension;
                //gallery.Hidden = Hidden;
                //gallery.FileSize = file.Length;

                if (gallery.Id == 0)
                    await _NarijeDBContext.Galleries.AddAsync(gallery);
                else
                    _NarijeDBContext.Galleries.Update(gallery);
                await _NarijeDBContext.SaveChangesAsync();
                Id = gallery.Id;

                /*
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.USEast1,
                    ServiceURL = "https://storage.iran.liara.space"
                };
                var amazonS3Client = new AmazonS3Client(
                  "i5o35ti6tssqid60",
                  "505e6142-72e5-41f8-a054-7039083facf3",
                  config);

                var fileTransferUtility =
                    new TransferUtility(amazonS3Client);
                await fileTransferUtility.UploadAsync(file.OpenReadStream(),
                                               "tahlilmobile-gallery", SysFileName);
                */

                string SysFileName = string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);
                var filePath = path + SysFileName;

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                await MakeImageParts(filePath, path, gallery);

                return Id;
            }

            return Id;

        }

        public static async Task<int?> EditFromGallery(NarijeDBContext _NarijeDBContext, int? Id, string ids)
        {

            if (ids == null)
                return Id;

            var id = ids.Split(",");
            if (id.Count() == 0)
                return Id;

            int n = Int32.Parse(id.FirstOrDefault());

            var gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == n).AsNoTracking().FirstOrDefaultAsync();

            if (gallery is null)
                return Id;

            return gallery.Id;

        }

        public static async Task<int?> AddFromGallery(NarijeDBContext _NarijeDBContext, string ids)
        {

            if (ids == null)
                return null;

            var id = ids.Split(",");
            if (id.Count() == 0)
                return null;

            int n = Int32.Parse(id.FirstOrDefault());

            var gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == n).AsNoTracking().FirstOrDefaultAsync();

            if (gallery is null)
                return null;

            return gallery.Id;

        }
    }
}
