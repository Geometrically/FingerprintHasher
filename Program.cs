﻿using System;
using Microsoft.Research.SEAL;
using OpenCvSharp;

namespace Fingercrypt
{
    class Program
    {
        public static void Main(string[] args)
        {
            HashFingerprint(null);
            
            var img = Cv2.ImRead("image.jpg", 0);
            Cv2.ImShow("Original", img);
            
            Cv2.BitwiseNot(img, img);
            Cv2.Threshold(img, img, 127, 255, ThresholdTypes.Binary);

            var lines = GetImageLines(img);
            
            Console.WriteLine(BitConverter.ToString(HashFingerprint(lines)).Length);
            
            Cv2.WaitKey(0);
        }

        public static LineSegmentPoint[] GetImageLines(Mat img)
        {
            var skeleton = new Mat(img.Size(), MatType.CV_8UC1, new Scalar(0));
            
            Cv2.Canny(img, skeleton, 255/3, 255);
            Cv2.ImShow("Skeleton", skeleton);

            return Cv2.HoughLinesP(skeleton, 1, Cv2.PI / 180, 15);
        }
        
        public static byte[] HashFingerprint(LineSegmentPoint[] lines)
        {
            var encryptionParams = new EncryptionParameters(SchemeType.BFV);

            encryptionParams.PolyModulusDegree = 4096;
            encryptionParams.CoeffModulus = CoeffModulus.BFVDefault(4096);
            encryptionParams.PlainModulus = new SmallModulus(512);
            
            var context = new SEALContext(encryptionParams);
            var keygen = new KeyGenerator(context);
            
            var encryptor = new Encryptor(context, keygen.PublicKey);
            
            var cipherText = new Ciphertext();
            encryptor.Encrypt(new Plaintext("1"), cipherText);
            
            Console.WriteLine(cipherText);
            
            return null;
        }
        
        /*

        public static byte[] HashFingerprint(LineSegmentPoint[] lines)
        {
            var fingerprintHash = new MemoryStream();
            
            foreach (var line in lines)
            {
                using (var algorithm = new SHA256Managed())
                {
                    var s = new MemoryStream();
                    
                    var p1XBytes = BitConverter.GetBytes(line.P1.X);
                    var p1YBytes = BitConverter.GetBytes(line.P1.Y);
                    
                    var p2XBytes = BitConverter.GetBytes(line.P2.X);
                    var p2YBytes = BitConverter.GetBytes(line.P2.Y);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(p1XBytes);
                        Array.Reverse(p1YBytes);
                        Array.Reverse(p2XBytes);
                        Array.Reverse(p2YBytes);
                    }
                    
                    s.Write(p1XBytes, 0, p1XBytes.Length);
                    s.Write(p1YBytes, 0, p1YBytes.Length);
                    s.Write(p2XBytes, 0, p2XBytes.Length);
                    s.Write(p2YBytes, 0, p2YBytes.Length);
                    
                    var hash = algorithm.ComputeHash(s);
                    
                    fingerprintHash.Write(hash, 0, hash.Length);
                }
            }

            return fingerprintHash.ToArray();
        }
        */
    }
}