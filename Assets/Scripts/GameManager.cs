using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour {

	public GameObject blockPrefab;
	public GameObject revealedBlockPrefab;
	public GameObject minePrefab;

	public GameObject gameOverText;
	public GameObject missionAccomplishedText;

	public int width = 10;
	public int height = 10;

	public int mineCount = 10;

	public Sprite[] numberSprites;
	public Color color1;
	public Color color2;

	public ParticleSystem[] explosions;

	bool[,] board; // true if mine, else false
	int[,] status; // 0: normal, 1: revealed, 2: flagged

	Block[,] blocks;
	Block selectedBlock;

	int flagCount;

	bool gameOver;

	void Start () {
		board = new bool[height, width];
		status = new int[height, width];
		blocks = new Block[height, width];

		SetupBoard();

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				GameObject blockObj = Instantiate(blockPrefab, new Vector3(i - 4.5f, 0.25f, j - 4.5f), Quaternion.identity) as GameObject;
				Block block = blockObj.GetComponent<Block>();
				block.x = i;
				block.y = j;
				blocks[j, i] = block;
			}
		}

		flagCount = mineCount;
		gameOver = false;
	}

	void Update() {
		if (!gameOver) {
			HandleBlockSelection();

			if (Input.GetButtonUp("Fire1")) {
				if (selectedBlock != null) {
					selectedBlock.SetUnSelected();
					RevealBlocks();
					selectedBlock = null;
				}
			}

			if (Input.GetButtonUp("Fire2")) {
				if (selectedBlock != null && selectedBlock.gameObject.activeSelf) {
					bool flag = status[selectedBlock.y, selectedBlock.x] == 0;

					if (flag && flagCount > 0) {
						status[selectedBlock.y, selectedBlock.x] = 2;
						selectedBlock.SetFlag(true);
						flagCount--;
						CheckMissionAccomplished();
					} else if (!flag && flagCount < mineCount) {
						status[selectedBlock.y, selectedBlock.x] = 0;
						selectedBlock.SetFlag(false);
						flagCount++;
					}
				}
			}
		} else {
			if (Input.anyKeyDown) {
				ReloadLevel();
			}
		}

	}

	void SetupBoard() {
		int count = mineCount;
		while (count > 0) {
			int x = Random.Range(0, width - 1);
			int y = Random.Range(0, height - 1);

			if (!board[y, x]) {
				board[y, x] = true;
				count--;
			}
		}
	}

	void RevealBlocks() {
		int x = selectedBlock.x;
		int y = selectedBlock.y;
		if (board[y, x] == true) {
			// game over
			ExplodeAllMines();
			gameOver = true;
			gameOverText.SetActive(true);
		} else if (status[y, x] == 2 || status[y, x] == 1) {
			// flagged or revealed, do not do anything
			return;
		} else {
			// calculate number of mines on conjunctive blocks
			int mines = CalculateMines(x, y);
			// reveal conjunctive blocks if no mines
			if (mines == 0) {
				status[y, x] = 1;
				blocks[y, x].gameObject.SetActive(false);
				Instantiate(revealedBlockPrefab, blocks[y, x].transform.position, Quaternion.identity);
				for (int i = -1; i < 2; i ++) {
					for (int j = -1; j < 2; j++) {
						if (i == 0 && j ==0) {
							continue;
						}
						int bx = x + i;
						int by = y + j;

						if (bx < 0 || bx >= width || by < 0 || by >= height) {
							continue;
						}

						if (status[by, bx] == 0 && CalculateMines(bx, by) == 0) {
							status[by, bx] = 1;
							blocks[by, bx].gameObject.SetActive(false);
							Instantiate(revealedBlockPrefab, blocks[by, bx].transform.position, Quaternion.identity);
							RevealConjunctiveEmptyBlocks(bx, by);
						}
					}
				}
			} else {
				status[y, x] = 1;
				blocks[y, x].gameObject.SetActive(false);
				GameObject revealedBlockGameObject = Instantiate(revealedBlockPrefab, selectedBlock.transform.position, Quaternion.identity) as GameObject;
				RevealedBlock revealedBlock = revealedBlockGameObject.GetComponent<RevealedBlock>();
				revealedBlock.SetImage(numberSprites[mines], Color.Lerp(color1, color2, mines / 8f));
			}
		}
	}

	void ExplodeAllMines() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (board[y, x]) {
					ParticleSystem explosion = Instantiate(explosions[Random.Range(0, explosions.Length)], blocks[y, x].transform.position, Quaternion.identity) as ParticleSystem;
					explosion.Play();
					Destroy(explosion.gameObject, explosion.duration);

					if (status[y, x] != 2) {
						blocks[y, x].gameObject.SetActive(false);
						Instantiate(minePrefab, blocks[y, x].transform.position, Quaternion.identity);
					} else {
						MeshRenderer[] renderers = blocks[y, x].gameObject.GetComponentsInChildren<MeshRenderer>();
						foreach (var renderer in renderers) {
							renderer.material.color = new Color(0.8f, 0.8f, 0.8f);
						}
					}
				}
			}
		}
	}

	void CheckMissionAccomplished() {

		int correctlyFlagged = 0;

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (board[y, x] && status[y, x] == 2) {
					correctlyFlagged++;
				}
			}
		}

		if (correctlyFlagged == mineCount) {
			gameOver = true;
			missionAccomplishedText.SetActive(true);
		}
	}

	void RevealConjunctiveEmptyBlocks(int x, int y) {

		for (int i = -1; i < 2; i ++) {
			for (int j = -1; j < 2; j++) {
				if (i == 0 && j ==0) {
					continue;
				}
				int bx = x + i;
				int by = y + j;

				if (bx < 0 || bx >= width || by < 0 || by >= height) {
					continue;
				}

				if (status[by, bx] == 0) {
					int mines = CalculateMines(bx, by);
					if (mines == 0) {
						status[by, bx] = 1;
						blocks[by, bx].gameObject.SetActive(false);
						Instantiate(revealedBlockPrefab, blocks[by, bx].transform.position, Quaternion.identity);
						RevealConjunctiveEmptyBlocks(bx, by);
					} else {
						status[by, bx] = 1;
						blocks[by, bx].gameObject.SetActive(false);
						GameObject revealedBlockObj = Instantiate(revealedBlockPrefab, blocks[by, bx].transform.position, Quaternion.identity) as GameObject;
						RevealedBlock revealedBlock = revealedBlockObj.GetComponent<RevealedBlock>();
						revealedBlock.SetImage(numberSprites[mines], Color.Lerp(color1, color2, mines / 8f));
					}
				} 
			}
		}
	}

	int CalculateMines(int x, int y) {
		int count = 0;
		for (int i = -1; i < 2; i ++) {
			for (int j = -1; j < 2; j++) {
				if (i == 0 && j ==0) {
					continue;
				}
				int bx = x + i;
				int by = y + j;

				if (bx < 0 || bx >= width || by < 0 || by >= height) {
					continue;
				}

				if (board[by, bx]) {
					count++;
				}
			}
		}
		return count;
	}

	void HandleBlockSelection() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground"))) {
			int z = (int)(hit.point.z + height / 2f);
			int x = (int)(hit.point.x + width / 2f);
			Block block = blocks[z, x];

			if (selectedBlock != null && selectedBlock != block) {
				selectedBlock.SetUnSelected();
				block.SetSelected();
				selectedBlock = block;
			} else {
				selectedBlock = block;
				block.SetSelected();
			}

		} else if (selectedBlock != null) {
			selectedBlock.SetUnSelected();
			selectedBlock = null;
		}
	}

	void ReloadLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//		Application.LoadLevel(Application.loadedLevel);
	}

}
