using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Construction_Grid_Generator : MonoBehaviour
{
   
    public GameObject grid_Node;

    public GameObject up_left_limit_Node;
    public GameObject down_right_limit_Node;

    
    //GRID RELATED VARIABLES
    List<List<GameObject>> grid_matrix;
    [Range(1, 20)]
    public float row_number  = 10f;
    [Range(1, 20)]
    public float column_number = 10f;
    float gap_between_elements = 5f;
    Vector3 startPosition;

    //MOUSE DETECTION RELATED VARIABLES
    public bool Allow_position_mouse = true;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        grid_matrix = new List<List<GameObject>>();
        //CreateNodeVector();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
       // if (Allow_position_mouse && Input.GetMouseButtonDown(0))
       // {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                GameObject objectHit = hit.transform.gameObject;
                grid_Node.transform.position = hit.point;
                //UpdateGridSelectionByMouse(hit.point);
                MousePositionByGridMatrix(hit.point,
                    up_left_limit_Node.transform.position,
                    down_right_limit_Node.transform.position);
            }

           // Debug.Log("Click Izquierdo Apretado");
           // DetectNodeFromMousePosition();
       // }
    }

    void CreateNodeVector()
    {
       startPosition = new Vector3(transform.position.x - gap_between_elements * row_number / 2, 0,
                                       transform.position.z - gap_between_elements * row_number / 2);
        grid_matrix.Clear();
        for (int i = 0; i < row_number; i++)
        {
            List<GameObject> new_row = new List<GameObject>();
            for (int j = 0; j < column_number; j++)
            {
                GameObject newObject = Instantiate(grid_Node, new Vector3(startPosition.x + gap_between_elements * i, 0,
                                                                      startPosition.z + gap_between_elements * j), Quaternion.identity, transform);
                new_row.Add(newObject);
            }
            grid_matrix.Add(new_row);
        }
    }

    void MousePositionByGridMatrix(Vector3 mousePos, Vector3 up_left, Vector3 down_right)
    {
        
          if (mousePos.x < up_left.x || mousePos.x > down_right.x ||
              mousePos.z > up_left.z || mousePos.z < down_right.z)
            { return;        }

        //Tamaño del grid en coordenadas.
        float grid_X_size = down_right.x - up_left.x;
        float grid_Z_size = up_left.z - down_right.z;
        
        //Tamaño de cada celda, dependiendo del tamaño del grid y el numero de celdas.
        float cell_X_size = grid_X_size / row_number;
        float cell_Z_size = grid_Z_size / column_number;


        /**
        //WTL = World to Local.
        //Debemos trasladar de posicion global a posicion local de la matriz. 

        float WTL_pos_x = mousePos.x;
        float WTL_pos_z = mousePos.z;

        //MTG = mouse to grid
        //sacamos posicion de raton, relativa al tamaño de celda, para saber en cual se encuentran.
        float MTG_pos_x = WTL_pos_x / cell_X_size;
        float MTG_pos_z = WTL_pos_z / cell_Z_size;

        //Sacamos los indices para trabajar con las matrices.
        int MTG_index_x = Mathf.RoundToInt(MTG_pos_x);
        int MTG_index_z = Mathf.RoundToInt(MTG_pos_z);


        grid_Node.transform.position = mousePos;
        //Debug.Log(MIG_pos_x + " for X coordinates. ");
        Debug.Log(MTG_index_z + " for z coordinates. ");
        //*/

        //Conversion de espacio de trabajo en plan textura? 
        // Multiplicacion de coordenadas por la mitad del espacio de trabajo + sumar esa mitad.
        // Ejemplo: cambio de -1 a 1, a 0 a 1: 
        //* (0.5) --> para cambiar de -1 a 1, a -0.5 a 0.5
        //+ (0.5) --> para cambiar de -0.5 a 0.5, a 0 a 1.

        float initial_mouse_Pos_X = mousePos.x;
        float initial_mouse_Pos_Z = mousePos.z;

        //De -size a size, de -1 a 1. Perdemos datos por falta de precision (0.03 de 1?)
        double position_inside_grid_X = ((initial_mouse_Pos_X * 1 / (grid_X_size / 2)) + (1 /(grid_X_size / 2)));
        double position_inside_grid_Z = ((initial_mouse_Pos_Z * 1 / (grid_Z_size / 2)) + 1 /(grid_Z_size / 2));
        
        //De 0 a 1.
        double normalized_pos_grid_X = (position_inside_grid_X * 0.5f) + 0.5f;
        double normalized_pos_grid_Z = (position_inside_grid_Z * 0.5f) + 0.5f;

        //Debug.Log(grid_X_size + " X size");
        //Debug.Log(grid_Z_size + " Z size");


        //Debug.Log(((down_right_limit_Node.transform.position.x * 1 / (grid_X_size / 2)) + (1 / (grid_X_size / 2))) + " X max");
        //Debug.Log(((up_left_limit_Node.transform.position.x * 1 / (grid_X_size / 2)) + (1 / (grid_X_size / 2))) + " X min");
        //Debug.Log(((down_right_limit_Node.transform.position.z * 1 / (grid_Z_size / 2)) + (1 / (grid_Z_size / 2))) + " Z min");
        //Debug.Log(((up_left_limit_Node.transform.position.z* 1 / (grid_Z_size / 2)) + (1 / (grid_Z_size / 2))) + " Z max");

        Debug.Log(normalized_pos_grid_X + "X coordinates");
        //Debug.Log(normalized_pos_grid_Z + "Z coordinates");

    }



    void UpdateGridSelectionByMouse(Vector3 impactPoint)
    {
        float min_X = grid_matrix[0][0].transform.position.x;
        float min_Z = grid_matrix[0][0].transform.position.z;
        float max_X = grid_matrix[grid_matrix.Count - 1][grid_matrix.Count - 1].transform.position.x;
        float max_Z = grid_matrix[grid_matrix.Count - 1][grid_matrix.Count - 1].transform.position.z;

        if (impactPoint.x < min_X || impactPoint.x > max_X ||
            impactPoint.z < min_Z || impactPoint.z > max_Z)
        {   return;    }

        float matrix_row_of_imp_point = (impactPoint.x / max_X);
        float matrix_col_of_imp_point = (impactPoint.z / max_Z);
        Debug.Log(matrix_row_of_imp_point + " for rows. ");
        Debug.Log(matrix_col_of_imp_point + " for cols. ");

    }

    

}
