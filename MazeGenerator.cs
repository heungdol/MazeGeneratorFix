using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {

	public GameObject blockPrefab;

	public int mazeWidth;
	public int mazeHeight;

	public Color mazeColor0;
	public Color mazeColor1;

	private GameObject[,] blocks;
	private IEnumerator currentCoroutine;
	private float fixedGapTime = 0.05f;

	public void StartGenerateMaze () {
		if (mazeWidth < 2 || mazeHeight < 2)
			return;
		
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		
		currentCoroutine = GenerateMaze (mazeWidth, mazeHeight, mazeColor0, mazeColor1);
		StartCoroutine (currentCoroutine);
	}

	IEnumerator GenerateMaze (int width, int height, Color color0, Color color1) {
		if (blocks != null) {
			for (int y = 0; y < blocks.GetLength (1); y++) {
				for (int x = 0; x < blocks.GetLength (0); x++) {
					Destroy (blocks [y, x].gameObject);
				}
			}
		}
			
		int totalWidth = (width * 2) + 1;
		int totalHeight = (height * 2) + 1;

		GameObject.FindWithTag ("MainCamera").GetComponent<CameraMove> ().SetDesiredPosition (totalWidth, totalHeight);

		int [,] intBlocks  = new int [totalHeight, totalWidth];	// 0: null space, 1: wall, 2: checked space or opened wall
		blocks = new GameObject[totalHeight, totalWidth];

		for (int y = 0; y < totalHeight; y++) {
			for (int x = 0; x < totalWidth; x++) {
				GameObject block = Instantiate (blockPrefab);

				if (x == 0 || x == totalWidth - 1 || y == 0 || y == totalHeight - 1 || x % 2 == 0 || y % 2 == 0) {
					intBlocks [y, x] = 1;
					block.GetComponent<MeshRenderer> ().material.color = color1;
				} else {
					intBlocks [y, x] = 0;
					block.GetComponent<MeshRenderer> ().material.color = color0;
				}

				block.transform.position = new Vector2 (x, y);
				block.name = "Block["+x+", "+y+"]";
				block.transform.SetParent (gameObject.transform);
				blocks [y, x] = block;
			}
		}
		// entrance
		intBlocks [0, 1] = 0;
		blocks[0, 1].GetComponent<MeshRenderer> ().material.color = color0;
		// exit
		intBlocks [totalHeight-1, totalWidth-2] = 0;
		blocks[totalHeight-1, totalWidth-2].GetComponent<MeshRenderer> ().material.color = color0;

		int currentX, currentY;
		currentX = 1 + UnityEngine.Random.Range (0, totalWidth / 2) * 2;
		currentY = 1 + UnityEngine.Random.Range (0, totalHeight / 2) * 2;

		while (true) {
			bool top = true;		// 0
			bool right = true;		// 1
			bool bottom = true;		// 2
			bool left = true;		// 3

			do {
				intBlocks [currentY, currentX] = 2;

				int currentDirection = UnityEngine.Random.Range(0, 4);
				int currentDirectionDelta = 1 + UnityEngine.Random.Range(0, 1) * 2;

				bool determindDirection = false;

				while ((top || right || bottom || left) && !determindDirection) {
					try {
						switch (currentDirection) {
						case 0 :
							if (intBlocks[currentY+2, currentX] == 0) {
								intBlocks [currentY+1, currentX] = 2;
								blocks[currentY+1, currentX].GetComponent<MeshRenderer>().material.color = color0;
							
								currentY += 2;
								determindDirection = true;
							} else {
								top = false;
							}
							break;
						case 1 :
							if (intBlocks[currentY, currentX+2] == 0) {
								intBlocks [currentY, currentX+1] = 2;
								blocks[currentY, currentX+1].GetComponent<MeshRenderer>().material.color = color0;

								currentX += 2;
								determindDirection = true;
							} else {
								right = false;
							}
							break;
						case 2 :
							if (intBlocks[currentY-2, currentX] == 0) {
								intBlocks [currentY-1, currentX] = 2;
								blocks[currentY-1, currentX].GetComponent<MeshRenderer>().material.color = color0;

								currentY -= 2;
								determindDirection = true;
							} else {
								bottom = false;
							}
							break;
						case 3 :
							if (intBlocks[currentY, currentX-2] == 0) {
								intBlocks [currentY, currentX-1] = 2;
								blocks[currentY, currentX-1].GetComponent<MeshRenderer>().material.color = color0;

								currentX -= 2;
								determindDirection = true;
							} else {
								left = false;
							}
							break;
						}
					} catch (IndexOutOfRangeException e) {
						print(e);
						switch (currentDirection) {
						case 0 :
							top = false;
							break;
						case 1 :
							right = false;
							break;
						case 2 :
							bottom = false;
							break;
						case 3 :
							left = false;
							break;
						}
					}
						
					if (determindDirection) {
						yield return new WaitForSeconds (fixedGapTime);
					}

					currentDirection += currentDirectionDelta;
					currentDirection %= 4;
				}

			} while (top || right || bottom || left);

			bool detect = false;

			for (int y = 1; y < totalHeight && !detect; y += 2) {
				for (int x = 1; x < totalWidth  && !detect; x += 2) {
					if (intBlocks [y, x] == 0) {
						int newDirection = UnityEngine.Random.Range (0, 4);
						int newDirectionDelta = 1 + 2 * UnityEngine.Random.Range (0, 1);

						bool newTop = true;
						bool newRight = true;
						bool newBottom = true;
						bool newLeft = true;

						while ((newTop || newRight || newBottom || newLeft) && !detect) {
							try {
								switch (newDirection) {
								case 0 :
									if (intBlocks [y + 2, x] == 2) {
										currentX = x;
										currentY = y + 2;
										detect = true;
									} else {
										newTop = false;
									}
									break;
								case 1 :
									if (intBlocks [y, x + 2] == 2) {
										currentX = x + 2;
										currentY = y;
										detect = true;
									} else {
										newRight = false;
									}
									break;
								case 2 :
									if (intBlocks [y - 2, x] == 2) {
										currentX = x;
										currentY = y - 2;
										detect = true;
									} else {
										newBottom = false;
									}
									break;
								case 3 :
									if (intBlocks [y, x - 2] == 2) {
										currentX = x - 2;
										currentY = y;
										detect = true;
									} else {
										newLeft = false;
									}
									break;
								}
							} catch (IndexOutOfRangeException e) {
								switch (newDirection) {
								case 0 :
									newTop = false;
									break;
								case 1 :
									newRight = false;
									break;
								case 2 :
									newBottom = false;
									break;
								case 3 :
									newLeft = false;
									break;
								}
							}

							newDirection += newDirectionDelta;
							newDirection %= 4;
						}
					}
				}
			}

			if (!detect)
				break;
		}
	}
}
