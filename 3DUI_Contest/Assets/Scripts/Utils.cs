using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

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
	
	public static float[] convertToFloat(Matrix4x4 m)
	{
		float[] v = {
			m.GetColumn(0).x, m.GetColumn(0).y, m.GetColumn(0).z, m.GetColumn(0).w,
			m.GetColumn(1).x, m.GetColumn(1).y, m.GetColumn(1).z, m.GetColumn(1).w,
			m.GetColumn(2).x, m.GetColumn(2).y, m.GetColumn(2).z, m.GetColumn(2).w,
			m.GetColumn(3).x, m.GetColumn(3).y, m.GetColumn(3).z, m.GetColumn(3).w
		};
		return v;
	}

	public static Matrix4x4 convertToMatrix(float[] f)
	{
		Matrix4x4 m = new Matrix4x4();

		m.SetColumn (0, new Vector4 (f [0], f [1], f [2], f [3]));
		m.SetColumn (1, new Vector4 (f [4], f [5], f [6], f [7]));
		m.SetColumn (2, new Vector4 (f [8], f [9], f [10], f [11]));
		m.SetColumn (3, new Vector4 (f [12], f [13], f [14], f [15]));

		return m;
	}

	public static String matrixString(float[] m)
	{
		return  "*0*" + m[0] + ","+ m[4] + "," + m[8] + ","+ m[12] +"\n"+
				"*1*" + m[1] + ","+ m[5] + ","+ m[9] + "," + m[13]  +"\n"+
				"*2*" + m[2] + ","+ m[6] + ","+ m[10] + "," + m[14]  +"\n"+
				"*3*" + m[3] + ","+ m[7] + ","+ m[11] + "," + m[15]+"\n";

	}
	public static float[] matrixToArray(float[,] m, int i, int size=16){
		float [] r = new float[size];
		for (int j=0; j<size; j++)
			r [j] = m [i,j];
		return r;
	}

}
