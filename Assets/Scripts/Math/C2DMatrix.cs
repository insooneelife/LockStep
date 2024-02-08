using UnityEngine;
using System.Collections;

class C2DMatrix
{
	class Matrix
	{
		public int v11, v12, v13;
		public int v21, v22, v23;
		public int v31, v32, v33;

		public Matrix()
		{
			v11 = 0; v12 = 0; v13 = 0;
			v21 = 0; v22 = 0; v23 = 0;
			v31 = 0; v32 = 0; v33 = 0;
		}

	};

	// Accessors to the matrix elements
	public int V11
	{
		get { return _matrix.v11; }
		set { _matrix.v11 = value; }
	}

	public int V12
	{
		get { return _matrix.v12; }
		set { _matrix.v12 = value; }
	}

	public int V13
	{
		get { return _matrix.v13; }
		set { _matrix.v13 = value; }
	}

	public int V21
	{
		get { return _matrix.v21; }
		set { _matrix.v21 = value; }
	}

	public int V22
	{
		get { return _matrix.v22; }
		set { _matrix.v22 = value; }
	}

	public int V23
	{
		get { return _matrix.v23; }
		set { _matrix.v23 = value; }
	}

	public int V31
	{
		get { return _matrix.v31; }
		set { _matrix.v31 = value; }
	}

	public int V32
	{
		get { return _matrix.v32; }
		set { _matrix.v32 = value; }
	}

	public int V33
	{
		get { return _matrix.v33; }
		set { _matrix.v33 = value; }
	}

	// Transforms a point from the agent's local space into world space
	public static Point2D PointToGlobalSpace(
		Point2D point,
		Point2D AgentHeading,
		Point2D AgentSide,
		Point2D AgentPosition)
	{
		// Make a copy of the point
		Point2D TransPoint = new Point2D(point.x, point.y);

		// Create a transformation matrix
		C2DMatrix matTransform = new C2DMatrix();

		// Rotate
		matTransform.Rotate(AgentHeading, AgentSide);

		// and translate
		matTransform.Translate(AgentPosition.x, AgentPosition.y);

		// Now transform the vertices
		return matTransform.TransformVector2Ds(TransPoint);
	}

	// Transforms a vector from the agent's local space into world space
	public static Point2D VectorToGlobalSpace(
		Point2D vec,
		Point2D AgentHeading,
		Point2D AgentSide)
	{
		// Make a copy of the point
		Point2D TransVec = vec;

		// Create a transformation matrix
		C2DMatrix matTransform = new C2DMatrix();

		// Rotate
		matTransform.Rotate(AgentHeading, AgentSide);

		// Now transform the vertices
		return matTransform.TransformVector2Ds(TransVec);
	}


	public static Point2D PointToLocalSpace(
		Point2D point,
		Point2D AgentHeading,
		Point2D AgentSide,
		Point2D AgentPosition)
	{
		// Make a copy of the point
		Point2D TransPoint = new Point2D(point.x, point.y);

		// Create a transformation matrix
		C2DMatrix matTransform = new C2DMatrix();

		int Tx = -Point2D.Dot(AgentPosition, AgentHeading);
		int Ty = -Point2D.Dot(AgentPosition, AgentSide);

		// Create the transformation matrix
		matTransform.V11 = AgentHeading.x; matTransform.V12 = AgentSide.x;
		matTransform.V21 = AgentHeading.y; matTransform.V22 = AgentSide.y;
		matTransform.V31 = Tx; matTransform.V32 = Ty;

		// Now transform the vertices
		return matTransform.TransformVector2Ds(TransPoint);
	}


	public static Point2D VectorToLocalSpace(
		Point2D vec,
		Point2D AgentHeading,
		Point2D AgentSide)
	{
		// Make a copy of the point
		Point2D TransPoint = vec;

		// Create a transformation matrix
		C2DMatrix matTransform = new C2DMatrix();

		// Create the transformation matrix
		matTransform.V11 = (AgentHeading.x); matTransform.V12 = (AgentSide.x);
		matTransform.V21 = (AgentHeading.y); matTransform.V22 = (AgentSide.y);

		// Now transform the vertices
		return matTransform.TransformVector2Ds(TransPoint);
	}


	public static Point2D Vec2DRotate(Point2D point, Point2D AgentHeading, Point2D AgentSide)
	{
		// Make a copy of the point
		Point2D TransPoint = point;

		// Create a transformation matrix
		C2DMatrix matTransform = new C2DMatrix();

		// Rotate
		matTransform.Rotate(AgentHeading, AgentSide);

		// Now transform the vertices
		return matTransform.TransformVector2Ds(TransPoint);
	}

	C2DMatrix()
	{
		_matrix = new Matrix();
		// Initialize the matrix to an identity matrix
		Identity();
	}

	// Create an identity matrix
	public void Identity()
	{
		_matrix.v11 = 1; _matrix.v12 = 0; _matrix.v13 = 0;

		_matrix.v21 = 0; _matrix.v22 = 1; _matrix.v23 = 0;

		_matrix.v31 = 0; _matrix.v32 = 0; _matrix.v33 = 1;
	}

	// Create a transformation matrix
	public void Translate(int x, int y)
	{
		Matrix mat = new Matrix();

		mat.v11 = 1; mat.v12 = 0; mat.v13 = 0;

		mat.v21 = 0; mat.v22 = 1; mat.v23 = 0;

		mat.v31 = x; mat.v32 = y; mat.v33 = 1;

		// and multiply
		MatrixMultiply(mat);
	}

	// Create a scale matrix
	public void Scale(int xScale, int yScale)
	{
		Matrix mat = new Matrix();

		mat.v11 = xScale; mat.v12 = 0; mat.v13 = 0;

		mat.v21 = 0; mat.v22 = yScale; mat.v23 = 0;

		mat.v31 = 0; mat.v32 = 0; mat.v33 = 1;

		// and multiply
		MatrixMultiply(mat);
	}

	// Create a rotation matrix from a fwd and side 2D vector
	public void Rotate(Point2D fwd, Point2D side)
	{
		Matrix mat = new Matrix();

		mat.v11 = fwd.x; mat.v12 = fwd.y; mat.v13 = 0;

		mat.v21 = side.x; mat.v22 = side.y; mat.v23 = 0;

		mat.v31 = 0; mat.v32 = 0; mat.v33 = 1;

		// and multiply
		MatrixMultiply(mat);
	}

	// Applys a transformation matrix to a point
	public Point2D TransformVector2Ds(Point2D vPoint)
	{
		int tempX = (_matrix.v11 * vPoint.x) + (_matrix.v21 * vPoint.y) + (_matrix.v31);

		int tempY = (_matrix.v12 * vPoint.x) + (_matrix.v22 * vPoint.y) + (_matrix.v32);

		vPoint.x = tempX;

		vPoint.y = tempY;

		return vPoint;
	}


	// Multiplies _matrix with mat
	private void MatrixMultiply(Matrix mat)
	{
		Matrix result = new Matrix();

		// First row
		result.v11 = (_matrix.v11 * mat.v11) + (_matrix.v12 * mat.v21) + (_matrix.v13 * mat.v31);
		result.v12 = (_matrix.v11 * mat.v12) + (_matrix.v12 * mat.v22) + (_matrix.v13 * mat.v32);
		result.v13 = (_matrix.v11 * mat.v13) + (_matrix.v12 * mat.v23) + (_matrix.v13 * mat.v33);

		// Second
		result.v21 = (_matrix.v21 * mat.v11) + (_matrix.v22 * mat.v21) + (_matrix.v23 * mat.v31);
		result.v22 = (_matrix.v21 * mat.v12) + (_matrix.v22 * mat.v22) + (_matrix.v23 * mat.v32);
		result.v23 = (_matrix.v21 * mat.v13) + (_matrix.v22 * mat.v23) + (_matrix.v23 * mat.v33);

		// Third
		result.v31 = (_matrix.v31 * mat.v11) + (_matrix.v32 * mat.v21) + (_matrix.v33 * mat.v31);
		result.v32 = (_matrix.v31 * mat.v12) + (_matrix.v32 * mat.v22) + (_matrix.v33 * mat.v32);
		result.v33 = (_matrix.v31 * mat.v13) + (_matrix.v32 * mat.v23) + (_matrix.v33 * mat.v33);

		_matrix = result;
	}

	private Matrix _matrix;
}
