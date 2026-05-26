using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// The robot that physically acts out the learner's program in the VR scene.
    /// Visual feedback is the core learning mechanism — children see consequences immediately.
    /// </summary>
    public class RobotCharacter : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed      = 2f;
        [SerializeField] private float rotateSpeed    = 180f;
        [SerializeField] private float stepDistance   = 1.5f;

        [Header("Feedback")]
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private ParticleSystem errorParticles;
        [SerializeField] private Animator        animator;

        private bool _running;

        public void ExecuteSequence(List<InstructionType> instructions)
        {
            if (_running) return;
            StartCoroutine(RunSequence(instructions));
        }

        public void PreviewRepeat(InstructionType instruction, int count)
        {
            if (_running) return;
            var list = new List<InstructionType>();
            for (int i = 0; i < count; i++) list.Add(instruction);
            StartCoroutine(RunSequence(list));
        }

        public void ExecuteConditional(bool conditionTrue, ActionType ifAction, ActionType elseAction)
        {
            ActionType chosen = conditionTrue ? ifAction : elseAction;
            StartCoroutine(RunAction(chosen));
        }

        private IEnumerator RunSequence(List<InstructionType> instructions)
        {
            _running = true;
            animator?.SetBool("Running", true);

            foreach (var inst in instructions)
            {
                yield return StartCoroutine(ExecuteInstruction(inst));
                yield return new WaitForSeconds(0.15f);
            }

            animator?.SetBool("Running", false);
            _running = false;
            successParticles?.Play();
        }

        private IEnumerator ExecuteInstruction(InstructionType inst)
        {
            switch (inst)
            {
                case InstructionType.MoveForward:
                    yield return MoveInDirection(transform.forward);
                    break;
                case InstructionType.TurnLeft:
                    yield return Rotate(-90f);
                    break;
                case InstructionType.TurnRight:
                    yield return Rotate(90f);
                    break;
                case InstructionType.PickUp:
                    animator?.SetTrigger("PickUp");
                    yield return new WaitForSeconds(0.8f);
                    break;
                case InstructionType.Jump:
                    animator?.SetTrigger("Jump");
                    yield return new WaitForSeconds(0.6f);
                    break;
            }
        }

        private IEnumerator RunAction(ActionType action)
        {
            animator?.SetTrigger(action.ToString());
            yield return new WaitForSeconds(1f);
        }

        private IEnumerator MoveInDirection(Vector3 dir)
        {
            Vector3 target = transform.position + dir * stepDistance;
            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator Rotate(float degrees)
        {
            Quaternion target = transform.rotation * Quaternion.Euler(0, degrees, 0);
            while (Quaternion.Angle(transform.rotation, target) > 0.5f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, rotateSpeed * Time.deltaTime);
                yield return null;
            }
        }

        public void ShowError() => errorParticles?.Play();
    }

    public enum InstructionType { MoveForward, TurnLeft, TurnRight, PickUp, Jump, Wait }
    public enum ActionType      { PickUpKey, WalkThrough, OpenDoor, Wait }
}
