/**
* CUDA Kernel Device code
*
* Computes the vector addition of A and B into C. The 3 vectors have the same
* number of elements numElements.
*/

extern "C" __global__ void
vectorAdd(const float *A, const float *B, float *C, int numElements)
{
	int i = blockDim.x * blockIdx.x + threadIdx.x;

	if (i < numElements)
	{
		C[i] = A[i] + B[i];
	}
}

/**
* CUDA Kernel Device code
*
* Decode color value from array A by color patern array B to byte array C.
*/
extern "C" __global__ void vectorObarvi(const int *A, const unsigned char *B, unsigned char *C, int numElements)//480*640,655535,640*480*4,640*480
{
	int i = blockDim.x * blockIdx.x + threadIdx.x;

	if (i < numElements)
	{
		int D = A[i] * 4;
		i *= 4;
		for (int j = 0; j < 4; j++)
		{
			C[i + j] = B[D + j];
		}
	}
}
/**
* CUDA Kernel Device code
*
* Decode color value from array A by color patern array B to byte array C. Decoded value of A in D.
*/
extern "C" __global__ void vectorObarviSource(const unsigned char *A, const unsigned char *B, unsigned char *C, int *D, int numElements)//480*640*4,655535,640*480*4,640*480
{
	int i = blockDim.x * blockIdx.x + threadIdx.x;
	int ic = i * 4;

	if (i < numElements)
	{
		D[i] = (int)(A[ic + 1] << 8 | A[ic]);
		int k = D[i] * 4;
		//int D = (A[i + 3] << 24 | A[i + 2] << 16 | A[i + 1] << 8 | A[i]) * 4;
		for (int j = 0; j < 4; j++)
		{
			C[ic + j] = B[k + j];
		}
	}
}
