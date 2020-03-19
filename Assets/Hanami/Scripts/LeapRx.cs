using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Leap;
using Leap.Unity;
using LookingGlass;
using UniRx;
using UniRx.Triggers;

public class LeapRx : MonoBehaviour {

    static LeapRx instance;
    static LeapRx Instance {
        get {
            if (!instance) instance = new GameObject("Core").AddComponent<LeapRx>();
            return instance;
        }
    }

    LeapServiceProvider _provider;
    LeapServiceProvider provider {
        get {
            if (_provider == null) {
                _provider = FindObjectOfType<LeapServiceProvider>();
            }
            return _provider;
        }
    }

    public static IObservable<Frame> updateStream {
        get {
            return Instance.UpdateAsObservable()
                           .Select(_ => Instance.provider.CurrentFrame)
                           .Publish().RefCount();
        }
    }
    public static IObservable<Hand> leftHandStream {
        get {
            return updateStream.Select(frame => frame.Hands)
                               .Select(hands => hands.Where(hand => hand.IsLeft).ToList())
                               .Where(hands => hands.Count > 0)
                               .Select(hands => hands.First())
                               .Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightHandStream {
        get {
            return updateStream.Select(frame => frame.Hands)
                               .Select(hands => hands.Where(hand => hand.IsRight).ToList())
                               .Where(hands => hands.Count > 0)
                               .Select(hands => hands.First())
                               .Publish().RefCount();
        }
    }

    public static IObservable<Hand> handStream {
        get {
            return Observable.Merge(leftHandStream, rightHandStream)
                             .Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftMoveStream {
        get {
            return leftHandStream.Where(hand => hand.PalmVelocity.ToVector3().magnitude >= 2.0f)
                                 .ThrottleFirst(System.TimeSpan.FromMilliseconds(250))
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightMoveStream {
        get {
            return rightHandStream.Where(hand => hand.PalmVelocity.ToVector3().magnitude >= 2.0f)
                                  .ThrottleFirst(System.TimeSpan.FromMilliseconds(250))
                                  .Publish().RefCount();
        }
    }

    public static IObservable<Hand> handMoveStream {
        get {
            return Observable.Merge(leftMoveStream, rightMoveStream).Publish().RefCount();
        }
    }
    
    public static IObservable<Hand> pushMoveStream {
        get {
            return handMoveStream.Where(hand => Instance.DirectionCheck(Vector3.forward, hand.PalmVelocity.ToVector3().normalized))
                                 .Where(hand => Instance.DirectionCheck(Vector3.forward, hand.PalmNormal.ToVector3()))
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftPushStream {
        get {
            return pushMoveStream.Where(hand => hand.IsLeft)
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightPushStream {
        get {
            return pushMoveStream.Where(hand => hand.IsRight)
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> pullMoveStream {
        get {
            return handMoveStream.Where(hand => Instance.DirectionCheck(Vector3.back, hand.PalmVelocity.ToVector3().normalized))
                                 .Where(hand => Instance.DirectionCheck(Vector3.back, hand.PalmNormal.ToVector3()))
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftPullStream {
        get {
            return pullMoveStream.Where(hand => hand.IsLeft)
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightPullStream {
        get {
            return pullMoveStream.Where(hand => hand.IsRight)
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftSlideStream {
        get {
            return leftMoveStream.Where(hand => Instance.DirectionCheck(Vector3.right, hand.PalmVelocity.ToVector3().normalized))
                                 .Where(hand => Instance.DirectionCheck(Vector3.right, hand.PalmNormal.ToVector3().normalized))
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightSlideStream {
        get {
            return rightMoveStream.Where(hand => Instance.DirectionCheck(Vector3.left, hand.PalmVelocity.ToVector3().normalized))
                                  .Where(hand => Instance.DirectionCheck(Vector3.left, hand.PalmNormal.ToVector3().normalized))
                                  .Publish().RefCount();
        }
    }

    public static IObservable<Hand> slideMoveStream {
        get {
            return Observable.Merge(leftSlideStream, rightSlideStream).Publish().RefCount();
        }
    }

    public static IObservable<Hand> upMoveStream {
        get {
            return handMoveStream.Where(hand => Instance.DirectionCheck(Vector3.up, hand.PalmVelocity.ToVector3().normalized))
                                 .Where(hand => Instance.DirectionCheck(Vector3.up, hand.PalmNormal.ToVector3().normalized))
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftUpStream {
        get {
            return upMoveStream.Where(hand => hand.IsLeft).Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightUpStream {
        get {
            return upMoveStream.Where(hand => hand.IsRight).Publish().RefCount();
        }
    }

    public static IObservable<Hand> downMoveStream {
        get {
            return handMoveStream.Where(hand => Instance.DirectionCheck(Vector3.down, hand.PalmVelocity.ToVector3().normalized))
                                 .Where(hand => Instance.DirectionCheck(Vector3.down, hand.PalmNormal.ToVector3().normalized))
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftDownStream {
        get {
            return upMoveStream.Where(hand => hand.IsLeft).Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightDownStream {
        get {
            return upMoveStream.Where(hand => hand.IsRight).Publish().RefCount();
        }
    }



    public static IObservable<Hand> grabHandStream {
        get {
            return handStream.Where(hand => hand.GrabStrength >= 0.5f).Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftGrabStream {
        get {
            return grabHandStream.Where(hand => hand.IsLeft).Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightGrabStream {
        get {
            return grabHandStream.Where(hand => hand.IsRight).Publish().RefCount();
        }
    }
    
    public static IObservable<Hand> rightOpenStream
    {
        get
        {
            return rightHandStream.SkipUntil(rightHandStream.Where(hand => hand.GrabStrength >= 0.8))
                .TakeUntil(rightHandStream.Where(hand => hand.GrabStrength <= 0.2f))
                .LastOrDefault()
                .RepeatUntilDestroy(Instance.gameObject)
                .Where(x => x != null)
                .Publish().RefCount();
        } 
        
    }

    public static IObservable<Hand> leftOpenStream
    {
        get
        {
            return leftHandStream.SkipUntil(leftHandStream.Where(hand => hand.GrabStrength >= 0.8))
                .TakeUntil(leftHandStream.Where(hand => hand.GrabStrength <= 0.2f))
                .LastOrDefault()
                .RepeatUntilDestroy(Instance.gameObject)
                .Where(x => x != null)
                .Publish().RefCount();
        }

    }

    public static IObservable<Finger> thumbsUpStream {
        get {
            return FingerStream(1, Finger.FingerType.TYPE_THUMB).Select(fingers => fingers.First())
                                                                .Where(finger => Instance.DirectionCheck(Vector3.up, finger.Direction.ToVector3()))
                                                                .Publish().RefCount();
        }
    }

    public static IObservable<List<Finger>> peaceSignStream {
        get {
            return FingerStream(2, Finger.FingerType.TYPE_INDEX, Finger.FingerType.TYPE_MIDDLE)
                .Where(fingers => fingers.All(finger => Instance.DirectionCheck(Vector3.up, finger.Direction.ToVector3())))
                .Publish().RefCount();
        }
    }

    public static IObservable<Hand> punchStream {
        get {
            return handMoveStream.Where(hand => hand.GrabStrength >= 0.8f)
                                 .Where(hand => Instance.DirectionCheck(Vector3.forward, hand.PalmVelocity.ToVector3().normalized))
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> leftBloomStream {
        get {
            return leftHandStream.SkipUntil(leftHandStream.Where(hand => Instance.DirectionCheck(Vector3.up, hand.PalmNormal.ToVector3())).Where(hand => hand.GrabStrength >= 0.8f))
                                 .TakeUntil(leftHandStream.Where(hand => Instance.DirectionCheck(Vector3.up, hand.PalmNormal.ToVector3())).Where(hand => hand.GrabStrength <= 0.2f))
                                 .LastOrDefault()
                                 .RepeatUntilDestroy(Instance.gameObject)
                                 .Where(x => x != null)
                                 .Publish().RefCount();
        }
    }

    public static IObservable<Hand> rightBloomStream {
        get {
            return rightHandStream.SkipUntil(rightHandStream.Where(hand => Instance.DirectionCheck(Vector3.up, hand.PalmNormal.ToVector3())).Where(hand => hand.GrabStrength >= 0.8f))
                                  .TakeUntil(rightHandStream.Where(hand => Instance.DirectionCheck(Vector3.up, hand.PalmNormal.ToVector3())).Where(hand => hand.GrabStrength <= 0.2f))
                                  .LastOrDefault()
                                  .RepeatUntilDestroy(Instance.gameObject)
                                  .Where(x => x != null)
                                  .Publish().RefCount();
        }
    }
    public static IObservable<Hand> bloomStream {
        get {
            return Observable.Merge(leftBloomStream, rightBloomStream).Publish().RefCount();
        }
    }

    public static IObservable<Hand> pinchStream {
        get {
            return handStream.Where(hand => hand.PinchStrength >= 0.8f).Publish().RefCount();
        }
    }

    // Metho
    public static IObservable<List<Finger>> RightFingerStream(int fingerNum, params Finger.FingerType[] fingerTypes) {
        return rightHandStream.Select(hand => hand.Fingers)
                              .Where(fingers => fingers.Count(finger => finger.IsExtended) == fingerNum)
                              .Select(fingers => fingers.Where(finger => finger.IsExtended).ToList())
                              .Where(fingers => fingers.All(finger => fingerTypes.Any(type => type == finger.Type)))
                              .Publish().RefCount();
    }
    public static IObservable<List<Finger>> LeftFingerStream(int fingerNum, params Finger.FingerType[] fingerTypes) {
        return leftHandStream.Select(hand => hand.Fingers)
                             .Where(fingers => fingers.Count(finger => finger.IsExtended) == fingerNum)
                             .Select(fingers => fingers.Where(finger => finger.IsExtended).ToList())
                             .Where(fingers => fingers.All(finger => fingerTypes.Any(type => type == finger.Type)))
                             .Publish().RefCount();
    }
    public static IObservable<List<Finger>> FingerStream(int fingerNum, params Finger.FingerType[] fingerTypes) {
        return Observable.Merge(RightFingerStream(fingerNum, fingerTypes), LeftFingerStream(fingerNum, fingerTypes)).Publish().RefCount();
    }

    // Helper Method
    bool DirectionCheck(Vector3 dir, Vector3 targetDir, float threshold) {
//        var cam = Camera.main.transform;
        var cam = Holoplay.Instance.cam.transform;
        var camDir = cam.TransformDirection(dir);
        return Vector3.Dot(camDir, targetDir) >= threshold;
    }
    bool DirectionCheck(Vector3 dir, Vector3 targetDir) {
        return DirectionCheck(dir, targetDir, 0.8f);
    }

}