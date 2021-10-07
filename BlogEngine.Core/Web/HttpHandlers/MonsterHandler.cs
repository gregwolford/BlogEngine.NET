#region Using

using System;
using System.Web;
using System.Net;
using System.Globalization;
using System.Web.Hosting;
using System.Drawing;
using System.IO;

#endregion

namespace BlogEngine.Core.Web.HttpHandlers
{
  /// <summary>
  /// Creates images of monsters based on a md5 hash. It is used
  /// for displaying avatar icons in the comments.
  /// <remarks>
  /// This handler is based on the MonsterID HttpHandler by Alexander Schuc
  /// http://blog.furred.net/page/MonsterID-Demo.aspx
  /// </remarks>
  /// </summary>
  public class MonsterHandler : IHttpHandler
  {

    #region Private fields

    private const string CACHE_PATH = "~/App_Data/monstercache/{0}";
    private const string PARTS_PATH = "~/pics/monsterparts";

    private static readonly string[] parts = new string[] { "legs", "hair", "arms", "body", "eyes", "mouth" };
    private static readonly int[] partcount = new int[] { 5, 5, 5, 15, 15, 10 };

    #endregion

    #region IHttpHandler Members

    /// <summary>
    /// Enables processing of HTTP Web requests by a custom HttpHandler that 
    /// implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
    /// </summary>
    /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides 
    /// references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
    public void ProcessRequest(HttpContext context)
    {
      if (String.IsNullOrEmpty(context.Request.QueryString["seed"]) || String.IsNullOrEmpty(context.Request.QueryString["size"]))
        return;

      string md5 = context.Request.QueryString["seed"];
      int seed = int.Parse(md5.Substring(0, 6), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
      int size = int.Parse(context.Request.QueryString["size"], CultureInfo.InvariantCulture);

      string filename = GetMonsterFilename(seed, size); //GetMonster(seed, size);

      if (!File.Exists(filename))
        CreateMonster(seed, size, filename);

      SetHttpHeaders(context, md5);
      context.Response.WriteFile(filename);
    }

    /// <summary>
    /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"></see> instance.
    /// </summary>
    /// <value></value>
    /// <returns>true if the <see cref="T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
    public bool IsReusable
    {
      get { return true; }
    }

    #endregion

    /// <summary>
    /// This will make the browser and server keep the output
    /// in its cache and thereby improve performance.
    /// </summary>
    private static void SetHttpHeaders(HttpContext context, string md5)
    {
      string etag = "\"" + md5.GetHashCode() + "\"";
      string incomingEtag = context.Request.Headers["If-None-Match"];

      context.Response.ContentType = "image/png";
      context.Response.Cache.SetCacheability(HttpCacheability.Public);
      context.Response.Cache.SetExpires(DateTime.Now.AddYears(1));
      context.Response.Cache.SetETag(md5);

      if (String.Compare(incomingEtag, etag) == 0)
      {
        context.Response.StatusCode = (int)HttpStatusCode.NotModified;
        context.Response.End();
      }
    }

    /// <summary>
    /// Gets the filename of the monster based on the seed and size.
    /// </summary>
    private static string GetMonsterFilename(int seed, int size)
    {
      CultureInfo info = CultureInfo.InvariantCulture;
      return Path.Combine(HostingEnvironment.MapPath(string.Format(info, CACHE_PATH, size)), string.Format(info, "{0}.png", seed));
    }

    /// <summary>
    /// Creates the monster image and saves it to the cache on disk.
    /// </summary>
    private static void CreateMonster(int seed, int size, string filename)
    {
      string sourcedir = HostingEnvironment.MapPath(PARTS_PATH);
      Random rnd = new Random(seed);
      int[] currentParts = new int[parts.Length];

      for (int i = 0; i < currentParts.Length; i++)
      {
        currentParts[i] = rnd.Next(1, partcount[i]);
      }

      using (Bitmap bmp = new Bitmap(size, size))
      {
        Bitmap overlay;
        using (Graphics gfx = Graphics.FromImage(bmp))
        {
          gfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
          gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

          for (int i = 0; i < currentParts.Length; i++)
          {
            using (overlay = new Bitmap(Path.Combine(sourcedir, string.Format(CultureInfo.InvariantCulture, "{0}_{1}.png", parts[i], currentParts[i]))))
            {
              Rectangle rect = new Rectangle(new Point(0), overlay.Size);
              gfx.DrawImage(overlay, new Rectangle(new Point(0), bmp.Size), rect, GraphicsUnit.Pixel);
            }
          }
        }

        string path = Path.GetDirectoryName(filename);
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);

        bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
      }
    }

  }
}
