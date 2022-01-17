using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgent : Agent {

    Rigidbody rBody;
    void Start() {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;
    public Transform smallTarget;

    private bool _touchedSmaller = false;


    public override void OnEpisodeBegin() {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0) {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        //this.transform.localPosition = new Vector3(Random.value * 8 - 4,
        //                                   0.5f,
        //                                   Random.value * 8 - 4);
        //this.rBody.angularVelocity = Vector3.zero;

        //this.rBody.velocity = Vector3.zero;


        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 8 - 4,
                                           0.5f,
                                           Random.value * 8 - 4);


        while (Vector3.Distance(this.transform.localPosition, Target.localPosition) < 3) {
            Target.localPosition = new Vector3(Random.value * 8 - 4,
                                               0.25f,
                                               Random.value * 8 - 4);
        }


        smallTarget.gameObject.SetActive(true);
        _touchedSmaller = false;


        smallTarget.localPosition = new Vector3(Random.value * 8 - 4,
                                           0.25f,
                                           Random.value * 8 - 4);

        while (Vector3.Distance(Target.localPosition, smallTarget.localPosition) < 3 || Vector3.Distance(this.transform.localPosition, smallTarget.localPosition) < 3) {
            smallTarget.localPosition = new Vector3(Random.value * 8 - 4,
                                               0.25f,
                                               Random.value * 8 - 4);
        }




    }

    public override void CollectObservations(VectorSensor sensor) {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(smallTarget.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        sensor.AddObservation(_touchedSmaller);
        //sensor.AddObservation(Vector3.Distance(this.transform.localPosition, .localPosition));
        //sensor.AddObservation(Vector3.Distance(this.transform.localPosition, smallTarget.localPosition));
    }

    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        if (!_touchedSmaller) {

            float distanceToSmallerTarget = Vector3.Distance(this.transform.localPosition, smallTarget.localPosition);


            if (distanceToSmallerTarget < 1.0f) {
                AddReward(1f);
                smallTarget.gameObject.SetActive(false);
                _touchedSmaller = true;
            }

        }


        if (_touchedSmaller) {
            AddReward(.001f / distanceToTarget);
        }


        if (distanceToTarget < 1.42f) {
            if (_touchedSmaller) {
                AddReward(1f);

            } else {
                //AddReward(-0.01f);
                AddReward(-1f);
            }
            EndEpisode();
        }

        if (this.transform.localPosition.y < 0) {

            //if (_touchedSmaller) {
            //    SetReward(-0.25f);
            //} else {
            //    SetReward(-0.1f);
            //}
            AddReward(-.25f);
            EndEpisode();
        }

        //print(MaxStep.ToString());

        //// Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

}