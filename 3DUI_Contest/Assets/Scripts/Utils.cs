using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;


// Handle IPS and IDS. Associate an unique ID for a client.
// If the client reconnects with the same IP the ID will be the same.

public class HandleConnections{
	
	public List<String> ips = new List<String> ();

	public int findIP(String ip){
		if (ips.Count > 0) {
			for (int i = 0; i < ips.Count; i++) {
				if (String.Compare (ips [i], ip) == 0)
					return i;
			}
		}
		return -1;
	}

	public int getId(String ip){
		int idOfIP = findIP (ip);
		if (idOfIP == -1) {
			ips.Add (ip);
			return ips.Count - 1;
		} else
			return idOfIP; 
	}

	public List<String> allIPsConnected(){
		return ips;
	}

	public void deleteList(){
		ips.Clear ();
		ips.TrimExcess ();
	}
}

// Store the accumulated transformations performed by all users.
// This transformations are applied in the manipulated object.
public class Transforms{
	public List<Matrix4x4> deviceMatrix = new List<Matrix4x4>();
	public Matrix4x4   rotateMatrix    = new Matrix4x4();
	public Matrix4x4   translateMatrix = new Matrix4x4();
	public Matrix4x4   scaleMatrix     = new Matrix4x4();
	public Matrix4x4   viewMatrix      = new Matrix4x4();
	public Matrix4x4   rotateCameraMatrix = new Matrix4x4();
	public Vector3   cameraPosition = new Vector3();
	public Vector3 boxPosition = new Vector3();
	public Vector3 boxPositionSmooth = new Vector3();
	public bool isCameraRotation = false;
	public Mutex mutex = new Mutex();
}

// For each client connected. Store all their transformations.
public class Client{
	public GameObject deviceObject;
	public Matrix4x4 deviceMatrix;
	public Quaternion deviceRotation;

	public GameObject deviceCamera;
	public Matrix4x4 deviceCameraMatrix;
	public Quaternion deviceCameraRotation;
	public Camera deviceCameraCamera;

	public Texture2D deviceColorTextureForGUI;

	public int isTranslation = 0;
	public int isRotation = 0;
	public int isScale = 0;

	public bool connected;
	public int prevColor = -1;
	public int color = -1;
	public int id = -1;

    public Vector3 totalTranslation = new Vector3(0, 0, 0);
    public Quaternion totalRotation = Quaternion.identity;
    public Quaternion totalRotationCamera = Quaternion.identity;
    public float totalScaling = 1;

	public Client(){
		this.deviceMatrix = Matrix4x4.identity;
		this.connected = true;
		this.deviceObject = null;
		this.deviceRotation = new Quaternion ();
		this.deviceCameraMatrix = Matrix4x4.identity;
		this.deviceCamera = null;
		this.deviceCameraRotation = new Quaternion ();
		this.deviceCameraCamera = new Camera ();

	}

	public Client(int id){
		this.id = id;
		this.deviceMatrix = Matrix4x4.identity;
		this.connected = true;
		this.deviceObject = null;
		this.deviceRotation = new Quaternion ();
		this.deviceCameraMatrix = Matrix4x4.identity;
		this.deviceCamera = null;
		this.deviceCameraRotation = new Quaternion ();
		this.deviceCameraCamera = new Camera ();
	}
}


internal static class Utils
{
	public static void FromMatrix4x4(this Transform transform, Matrix4x4 matrix)
	{
		transform.localScale = matrix.GetScale();
		transform.rotation = matrix.GetRotation();
		transform.position = matrix.GetPosition();
	}
	
	public static Quaternion GetRotation(this Matrix4x4 matrix)
	{
		var qw = Mathf.Sqrt(1f + matrix.m00 + matrix.m11 + matrix.m22) / 2;
		var w = 4 * qw;
		var qx = (matrix.m21 - matrix.m12) / w;
		var qy = (matrix.m02 - matrix.m20) / w;
		var qz = (matrix.m10 - matrix.m01) / w;
		
		return new Quaternion(qx, qy, qz, qw);
	}


	public static System.Random random = new System.Random();
	public static float rand(){
		return (float)(random.NextDouble () * 2.0f - 1.0f);
	}

	public static Vector3 RandomUnitVector(){
		return new Vector3 (rand(), rand(),rand()).normalized;

	}


	public static Vector3 GetPosition(this Matrix4x4 matrix)
	{
		var x = matrix.m03;
		var y = matrix.m13;
		var z = matrix.m23;
		
		return new Vector3(x, y, z);
	}
	
	public static Vector3 GetScale(this Matrix4x4 m)
	{
		var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
		var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
		var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);
		
		return new Vector3(x, y, z);
	}
	
	public static float[] ConvertToFloat(Matrix4x4 m)
	{
		float[] v = {
			m.GetColumn(0).x, m.GetColumn(0).y, m.GetColumn(0).z, m.GetColumn(0).w,
			m.GetColumn(1).x, m.GetColumn(1).y, m.GetColumn(1).z, m.GetColumn(1).w,
			m.GetColumn(2).x, m.GetColumn(2).y, m.GetColumn(2).z, m.GetColumn(2).w,
			m.GetColumn(3).x, m.GetColumn(3).y, m.GetColumn(3).z, m.GetColumn(3).w
		};
		return v;
	}

	public static Matrix4x4 ConvertToMatrix(float[] f)
	{
		Matrix4x4 m = new Matrix4x4();

		m.SetColumn (0, new Vector4 (f [0], f [1], f [2], f [3]));
		m.SetColumn (1, new Vector4 (f [4], f [5], f [6], f [7]));
		m.SetColumn (2, new Vector4 (f [8], f [9], f [10], f [11]));
		m.SetColumn (3, new Vector4 (f [12], f [13], f [14], f [15]));

		return m;
	}



	public static float distMatrices(Matrix4x4 a, Matrix4x4 b)
	{
		float r = 0;
		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < 4; j++) {
				float v = a [i, j] - b [i, j];
				r += v * v;
			}
		}
		return (float)Math.Sqrt (r);
	}



    public static String ConvertToString(float[] m)
    {
        return "*0*" + m[0] + "," + m[4] + "," + m[8] + "," + m[12] + "\n" +
                "*1*" + m[1] + "," + m[5] + "," + m[9] + "," + m[13] + "\n" +
                "*2*" + m[2] + "," + m[6] + "," + m[10] + "," + m[14] + "\n" +
                "*3*" + m[3] + "," + m[7] + "," + m[11] + "," + m[15] + "\n";

    }

    public static String matrixString(Matrix4x4 matrix)
    {

        float[] m = ConvertToFloat(matrix);
        return "*0*" + m[0] + "," + m[4] + "," + m[8] + "," + m[12] + "\n" +
                "*1*" + m[1] + "," + m[5] + "," + m[9] + "," + m[13] + "\n" +
                "*2*" + m[2] + "," + m[6] + "," + m[10] + "," + m[14] + "\n" +
                "*3*" + m[3] + "," + m[7] + "," + m[11] + "," + m[15] + "\n";

    }
	/*public static float[] ConvertToFloat(float[,] m, int i, int size=16){
		float [] r = new float[size];
		for (int j=0; j<size; j++)
			r [j] = m [i,j];
		return r;
	}*/

    public static float[] ConvertToFloat(byte[] array, int offSet, int size)
    {

        float[] floats = new float[size / 4];

        for (int i = 0; i < size / 4; i++)
            floats[i] = BitConverter.ToSingle(array, i * 4 + offSet);

        return floats;
    }

    public static Matrix4x4 ConvertToMatrix(byte[] array, int offSet)
    {
        return ConvertToMatrix(ConvertToFloat(array, offSet, 64));
    }


    public static bool isNaN(Quaternion q)
    {
        return (
			float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w) ||
			float.IsInfinity(q.x) || float.IsInfinity(q.y) || float.IsInfinity(q.z) || float.IsInfinity(q.w)

		);
    }

    public static Quaternion NormalizeQuaternion(Quaternion q)
    {
        float sum = 0;
        for (int i = 0; i < 4; ++i)
            sum += q[i] * q[i];
        float magnitudeInverse = 1 / Mathf.Sqrt(sum);
        for (int i = 0; i < 4; ++i)
            q[i] *= magnitudeInverse;
        return q;
    }


    public static Color32 HexColor(int HexVal, float alpha)
    {
        byte B = (byte)((HexVal >> 24) & 0xFF);
        byte G = (byte)((HexVal >> 16) & 0xFF);
        byte R = (byte)((HexVal >> 8) & 0xFF);
        byte A = (byte)((HexVal) & 0xFF);
        return new Color32(R, G, B, (byte)(int)(A * alpha));
    }

    /*public static Matrix4x4 matrix(Quaternion q)
    {

        Matrix4x4 a = new Matrix4x4();
        a.SetRow(0, new Vector4(q.w, q.z, -q.y, q.x));
        a.SetRow(1, new Vector4(-q.z, q.w, q.x, q.y));
        a.SetRow(2, new Vector4(q.y, -q.x, q.w, q.z));
        a.SetRow(3, new Vector4(-q.x, -q.y, -q.z, q.w));

        Matrix4x4 b = new Matrix4x4();
        b.SetRow(0, new Vector4(q.w, q.z, -q.y, -q.x));
        b.SetRow(1, new Vector4(-q.z, q.w, q.x, -q.y));
        b.SetRow(2, new Vector4(q.y, -q.x, q.w, -q.z));
        b.SetRow(3, new Vector4(q.x, q.y, q.z, q.w));

        return b * a;
    }*/


/*	public static Interpolate(this Vector3 pR, this Quaternion rR, Vector3 pA, Quaternion rA, Vector3 pB, Quaternion rB, float f){
		rR = Quaternion.Slerp(rA, rB, f);
		pR = Vector3.Lerp(pA, pB, f); 
	}
*/

    static public Texture2D MakeTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }	
}
