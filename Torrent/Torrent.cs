using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace BitTorrent
{
    /* */
    public class Torrent
    {
       public string Name { get; private set; }
       public bool? IsPirvate { get; private set; }
       public List<FileItem> Files { get; private set; } = new List<FileItem>();
       public string FileDirectory { get { return (Files.Count > 1 ? Name + Path.DirectorySeparatorChar : ""); } }
       public string DownloadDirectory { get; private set; }
       public List<Tracker> Tracker { get; } = new List<Tracker>();
       public string Comment { get; set; }
       public string CreatedBy { get; set; }
       public DateTime CreationDate { get; set; }
       public Encoding Encoding { get; set; }
       public int BlockSize { get; private set; }
       public int PieceSize { get; private set; }
       public byte[][] PieceHashes { get; private set; }

       
       /* Hashes */
       public byte[] Infohash { get; private set; } = new byte[20];
       public string HexStringInfoHash
       {
           get { return String.Join("", this.Infohash.Select(x => x.ToString("x2")));  }
       }
       public string UrlSafeStringInfoHash
       {
           get { return Encoding.UTF8.GetString(WebUtility.UrlEncodeToBytes(this.Infohash, 0, 20)); }
       }
       
       /* Blocks and Pieces */
       public int blockSize { get; private set; }
       public int pieceSize { get; private set; }
       public long totalSize { get { return Files.Sum(x => x.Size); } }
       
       public string FormattedPieceSize
       {
           get { return BytesToString(pieceSize); }
       }

       public string FormattedTotalSize
       {
           get { return BytesToString(totalSize); }
       }
       
       public int PieceCount { get { return PieceHashes.Length; } }
       
       public byte[] pieceHashes { get; private set; }
       public bool[] IsPlaceVerified { get; private set; }
       public bool[][] IsBlockAquired { get; private set; }
       
       public string VerifiedPiecesString
       {
           get { return String.Join("", IsPlaceVerified.Select(x => x ? 1 : 0)); }
       }
       public int VerifiedPieceCount { get { return IsPlaceVerified.Count(x => x); } }
       public double VerifiedRatio
       {
           get { return VerifiedPieceCount / (double)PieceCount; }
       }
       public bool IsCompleted { get { return VerifiedPieceCount == PieceCount; } }
       public bool IsStarted { get { return VerifiedPieceCount > 0; } }

       public long Uploaded { get; set; } = 0;
       public long Downloaded
       {
           get { return pieceSize * VerifiedPieceCount; }
       } 
       public long Left
       {
           get { return totalSize - Downloaded; }
       }

       public int GetPieceSize(int piece)
       {
           if (piece == PieceCount - 1)
           {
               int remainder = Convert.ToInt32(totalSize % PieceSize);
               if (remainder != 0)
                   return remainder;
           }

           return PieceSize;
       }

       public int GetBlockSize(int piece, int block)
       {
           if (block == GetBlockCount(piece) - 1)
           {
               int remainder = Convert.ToInt32(GetPieceSize(piece) % BlockSize);
               if(remainder != 0)
                   return remainder;
           }
           return BlockSize;
       }

       public int GetBlockCount(int piece)
       {
           return Convert.ToInt32(Math.Ceiling(GetPieceSize(piece) / (double)BlockSize));
       }
    }

    public class FileItem
    {
        public string Path;
        public long Size;
        public long Offset;
        
        public string FormattedSize
        {
            get { return Torrent.BytesToString(Size); }
        }
    }

    public class Tracker
    {
        public event EventHandler<List<IPEndPoint>> PeerListUpdated;
        public string Address { get; private set; }

        public Tracker(string address)
        {
            Address = address;
        }
    }
}