using CencLibrary;


const string inputFileName = @"C:\Users\mniki\Downloads\Ncps\gpac\Decode\2.cenc";
const string outputFileName = @"C:\Temp\2.mp4";
byte[] key = new byte[] { 0xF7, 0xC6, 0x11, 0x9C, 0x95, 0x32, 0x22, 0x1C, 0xFB, 0x1C, 0x7B, 0x88, 0x40, 0x3A, 0xDF, 0x96 };

Console.WriteLine($"Reading {inputFileName}.");
var start = DateTime.Now;

using var input = File.OpenRead(inputFileName);
using var output = File.OpenWrite(outputFileName);
var decr = new CencDecryptor();
var boxes = decr.Decrypt(input, output, key);

var elapsed = DateTime.Now - start;
Console.WriteLine($"Read {boxes} boxes in {elapsed.TotalSeconds} sec.");
