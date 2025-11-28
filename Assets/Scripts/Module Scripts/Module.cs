using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour 
{
    public int inputSockets;
    public int outputSockets;

    public List<Neuron> inputNeurons = new List<Neuron>();
    public List<Neuron> outputNeurons = new List<Neuron>();


    public Organism organism;

    public NeuralNetwork network;
    public virtual void AddSockets()
    {

    }
    public virtual void UpdateModule()
    {

    }

    protected void Awake()
    {
        organism = gameObject.transform.root.GetComponentInChildren<Organism>();
        network = organism.network;
    }
    public void AddToOrganism(Organism organism)
    {
        //Add function to organism to get free input neurons 
        inputNeurons = organism.GetFreeInputNeurons();
        outputNeurons = organism.GetFreeOutputNeurons();
  
        if (inputNeurons.Count >= inputSockets && outputNeurons.Count >= outputSockets)
        {
          
            organism.modules.Add(this);
            for (int i = 0; i < inputSockets; i++) {

                inputNeurons[i].module = this;
        
            }

            for (int i = 0; i < outputSockets; i++)
            {
                
                outputNeurons[i].module = this;
        
            }

        }
        else
        {
            Debug.Log("NOT ENOUGH SOCKETS");
            inputNeurons = null;
            outputNeurons = null;
        }
  
      


    }
}
