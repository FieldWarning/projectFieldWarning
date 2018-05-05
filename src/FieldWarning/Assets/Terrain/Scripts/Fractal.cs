using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

public class Fractal {
    public static List<float> getCutOffLevels(float[,] map, float[] levels) {
        float[] arr = new float[map.Length];
        System.Buffer.BlockCopy(map, 0, arr, 0, sizeof(float)*map.Length);
        Array.Sort(arr);
		float levelSum = 0;
		foreach(float a in levels){
			levelSum+=a;
		}
		List<float> cutOffLevels = new List<float>();
		float t = 0;
        
		for (int i=0;i< levels.Length - 1;i++) {
			t += levels[i];
			cutOffLevels.Add(arr[Mathf.FloorToInt(arr.Length * t / levelSum)]);
			
		}

		cutOffLevels.Add( arr[arr.Length - 1]);
        
		return cutOffLevels;
	}
	public static float[,] fractal(int n, int m, FractalOptions options){
        
		int size = Mathf.FloorToInt(Mathf.Max(n, m));
		
		int width = nextPowerOf2(size);

		int height = nextPowerOf2(size);


		float[,] map=new float[width+1,height+1];
        for (var i=0;i<width+1;i++){         
            for (var j=0;j<height+1;j++){
                map[i, j] = Single.NaN;
            }
        }
		
		
		int generation=0;
		
		
		
        int xLength = width>>1;
        int yLength = height>>1;
		float h=options.getScale(xLength);
        //System.UnityEngine.Random UnityEngine.RandomNumber = new System.UnityEngine.Random();
		
        map[0,0]=h*(getRandomNumber()-0.5f);
        map[0,height] = h*(getRandomNumber()-0.5f);
        map[width,0] = h*(getRandomNumber()-0.5f);
        map[width,height] =h*(getRandomNumber()-0.5f);
       
        while (xLength > 0)
        {
			
			//Debug.Log(xLength);
			//diamond phase
			//float lengthScale = xLength;//xLength*Terrain.activeTerrain.terrainData.heightmapScale.x;
			h = options.getScale(xLength);
			int x = xLength;
            while (x < width)
            {
				int y = yLength;
                while (y < height)
                {
                    
                    if (Single.IsNaN(map[x, y]))
                    {
                        map[x, y] = (map[x - xLength, y - yLength]+
                            map[x + xLength, y - yLength]+
                            map[x + xLength, y + yLength]+
                            map[x - xLength, y + yLength])/4f+
                            h * (getRandomNumber()-0.5f);
                            
                    }
					y += 2 * yLength;
                }
				 x += 2 * xLength;
            }
			//h =getScaleFactor(generation+0.5f);
            //square phase
			x = 0;
            while ( x <= width)
            {
				int y= 0;
                while ( y <= height )
                {
                    if (Single.IsNaN(map[x, y]))
                    {
						
                        if (y==0)
                        {
                            map[x, y] = (map[x, y + yLength] +
                                map[x + xLength, y] +
                                map[x - xLength, y]) / 3f +
                                h * (getRandomNumber() - 0.5f);
                        }
                        else if (y == height)
                        {
                            map[x, y] = (map[x, y - yLength] +
                                map[x + xLength, y] +
                                map[x - xLength, y]) / 3f +
                                h * (getRandomNumber() - 0.5f);
                        }
                        else if (x == 0)
                        {
                            map[x, y] = (map[x, y - yLength] +
                                map[x, y + yLength] +
                                map[x + xLength, y]) / 3f +
                                h * (getRandomNumber() - 0.5f);
                        }
                        else if (x == width)
                        {
                            map[x, y] = (map[x, y - yLength] +
                                map[x, y + yLength] +
                                map[x - xLength, y]) / 3f +
                                h * (getRandomNumber() - 0.5f);
                        }
                        else
                        {
							
							/*if(Single.IsNaN(map[x][ y - yLength])){
								trace(x+", "+(y - yLength)+" has not been assigned");
							}
							if(Math.isNaN(map[x][ y + yLength])){
								trace(x+", "+(y + yLength)+" has not been assigned");
							}
							if(Math.isNaN(map[x + xLength][ y])){
								trace("a");
							}
							if(Math.isNaN(map[x - xLength][ y])){
								trace("b");
							}*/
							
							
                            map[x, y] = (map[x, y - yLength] +
                                map[x, y + yLength] +
                                map[x + xLength, y] +
                                map[x - xLength, y]) / 4f +
                                h * (getRandomNumber() - 0.5f);
                        }
                    }
					y += yLength;
                }
				x += xLength;
            }
            xLength=xLength >> 1;
            yLength=yLength >> 1;
			generation+=1;
            
			
		}
        var outMap =new float[n,m];
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                outMap[i,j] = map[i,j];
            }
        }
        //Fractal.printToFile(map, "map.m");
        return outMap;
		
	}
	static private int nextPowerOf2(int x) {
		var l = Mathf.CeilToInt(Mathf.Log(x) / Mathf.Log(2));
		return Mathf.RoundToInt(Mathf.Pow(2, l));
		
	}
	//UnityEngine.Random number wrapper for later inclusion of seeds etc.
	static private float getRandomNumber() {
        return UnityEngine.Random.value;//Math.UnityEngine.Random();
	}
    public static void printToFile(float[,] data, string filename){
        using (FileStream fs = File.Create(filename))
        {
            string s="M=[";
            for (var r = 0; r < data.GetLength(0);r++ )
            {
                for (var c = 0; c < data.GetLength(1);c++)
                {
                    s += data[r,c] + "\t";
                }
                s += ";\n";
            }
            s+="]";
            Byte[] info = new UTF8Encoding(true).GetBytes(s);
            // Add some information to the file.
            fs.Write(info, 0, info.Length);
            fs.Flush();
            fs.Close();
        }
    }
    public static Point arbitraryDistribution(ref float[,] distribution){
        var width = distribution.GetLength(0);
        var height = distribution.GetLength(1);
        var horizontal = new float[width];
        float horizontalSum = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                horizontal[i] += distribution[i, j];
                horizontalSum += distribution[i, j];
            }
        }
        //pick X
        var r = getRandomNumber();
        int x=0;
        while (r > horizontal[x] / horizontalSum)
        {
            r -= horizontal[x] / horizontalSum;
            x++;
        }
        //pick Y
        r = getRandomNumber();
        int y = 0;
        while (r > distribution[x, y] / horizontal[x])
        {
            r -= distribution[x, y] / horizontal[x];
            y++;
        }
        var p = new Point();
        p.x = x;
        p.y = y;
        return p;

    }
    public struct Point : IEquatable<Point>
    {
        public int x;
        public int y;
        public bool Equals(Point other)
        {
            return x == other.x && y == other.y;
        }
        public Vector3 getMiddlePoint(Point other)
        {
            return new Vector3((x + other.x) / 2f,0, (y + other.y) / 2f);
        }
        public static bool isSame(uint d1, uint d2, bool diag)
        {
            var mod=4;
            if(diag)mod=8;
            return (d1 - d2) % mod == 0;
        }
        public static bool isNonDiagonal(uint d, bool diag)
        {
            if (!diag) return true;
            else return d % 2 == 0;
        }
        public uint getDirection(Point other, bool diag)
        {
            if (diag)
            {
                return _getDirectionDiag(other);
            }
            else
            {
                return _getDirection(other);
            }
        }
        private uint _getDirection(Point other)
        {
            if(other.y>y&&other.x==x){
                return 0;
            }
            else if(other.y==y&&other.x>x){
                return 1;
            }
            else if(other.y<y&&other.x==x){
                return 2;
            }
            else if (other.y == y && other.x < x)
            {
                return 3;
            }
            else{
                Debug.LogError("no such driection");
                return 0;
            }
        }
        private uint _getDirectionDiag(Point other)
        {
            if(other.y>y&&other.x==x){
                return 0;
            }
                else if(other.y>y&&other.x>x){
                return 1;
            }
            else if(other.y==y&&other.x>x){
                return 2;
            }
                else if(other.y<y&&other.x>x){
                return 3;
            }
            else if(other.y<y&&other.x==x){
                return 4;
            }
                else if(other.y<y&&other.x<x){
                return 5;
            }
            else if (other.y == y && other.x < x)
            {
                return 6;
            }
            else if (other.y > y && other.x < x)
            {
                return 7;
            }
            else{
                Debug.LogError("no such driection");
                return 0;
            }
        }
        public Point getPointFromDirection(uint d, bool diag)
        {
            if (diag)
            {
                return _getPointFromDirectionDiag(d);
            }
            else
            {
                return _getPointFromDirection(d);
            }
        }
        private Point _getPointFromDirection(uint d)
        {
            d %= 4;
            int dx = 0;
            int dy = 0;
            switch (d)
            {
                case 0:
                    dy = 1;
                    break;
                case 1:
                    dx = 1;
                    break;
                case 2:
                    dy = -1;
                    break;
                case 3:
                    dx = -1;
                    break;
                default:
                    Debug.LogError("no such driection");
                    break;
            }
            var p = this;
            p.x += dx;
            p.y += dy;
            return p;

        }
        private Point _getPointFromDirectionDiag(uint d)
        {
            d %= 8;
            int dx = 0;
            int dy = 0;
            switch (d)
            {
                case 0:
                    dy = 1;
                    break;
                case 1:
                    dx = 1;
                    dy = 1;
                    break;
                case 2:
                    dx = 1;
                    break;
                case 3:
                    dx = 1;
                    dy = -1;
                    break;
                case 4:
                    dy = -1;
                    break;
                case 5:
                    dx = -1;
                    dy = -1;
                    break;
                case 6:
                    dx = -1;
                    break;
                case 7:
                    dx = -1;
                    dy = 1;
                    break;
                default:
                    Debug.LogError("no such driection");
                    break;
            }
            var p = this;
            p.x += dx;
            p.y += dy;
            return p;

        }
    }
	
}
public class FractalOptions 
{
    float smallScale;//length of the smallest features of the map in tiles
    float largeScale;//length of the largest features of the map in tiles

    public FractalOptions(float smallScale, float largeScale)
    {
        this.smallScale = smallScale;
        this.largeScale = largeScale;

    }
    public float getScale(float generation)
    {
        var c = -(largeScale - smallScale) * (smallScale - largeScale) / 4;
        var f = (generation - smallScale) * (generation - largeScale) / c;
        return Mathf.Exp(-f);
    }
    
}

