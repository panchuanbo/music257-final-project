//
//  ViewController.swift
//  balance-ios
//
//  Created by Chuanbo Pan on 6/1/18.
//  Copyright © 2018 Chuanbo Pan. All rights reserved.
//

import UIKit
import CoreMotion
import SwiftSocket
import AVFoundation

class ViewController: UIViewController {
    
    // Labels
    @IBOutlet weak var infoLabel: UILabel!
    
    // Manager
    private let motionManager = CMMotionManager.init()
    
    // Socket
    let socket = TCPClient.init(address: "tcp://0.tcp.ngrok.io", port: 18240)
    let socketQueue = DispatchQueue.init(label: "socket")
    
    let socketLock = DispatchSemaphore.init(value: 1)
    
    private var connected = false
    
    // Sound
    var tone: AVTonePlayerUnit = AVTonePlayerUnit(harmonics: 2)
    var engine: AVAudioEngine = AVAudioEngine()
    
    // Small Song
    let audio: [AudioNotes] = [
        .E4_N, .D4_N, .C4_N, .D4_N, .E4_N, .E4_N, .E4_N, .D4_N, .D4_N, .D4_N, .E4_N, .G4_N, .G4_N
    ]
    let time: [Int] = [1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 2]
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        
        _setupMotion()
        _setupSound()
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    
    // MARK: - Sound
    
    private func _setupSound() {
        let format = AVAudioFormat(standardFormatWithSampleRate: tone.sampleRate, channels: 1)
        
        engine.attach(tone)
        let mixer = engine.mainMixerNode
        engine.connect(tone, to: mixer, format: format)
        do {
            try engine.start()
            
            tone.preparePlaying()
            tone.play()
            engine.mainMixerNode.volume = 1.0
        } catch let error as NSError {
            print(error)
        }
        
        var counter = 0
        Timer.scheduledTimer(withTimeInterval: 1.0, repeats: true) { (_) in
            self.tone.frequency = self.audio[counter % self.audio.count].rawValue
            counter += 1
            
            self.tone.stop()
            self.tone.play()
        }
    }
    
    // MARK: - Motion Management
    
    private func _setupMotion() {
        motionManager.startDeviceMotionUpdates(to: OperationQueue.main) { (data, error) in
            if let attitude = data?.attitude {
                let str = "pitch: \(attitude.pitch.truncate(places: 2)) yaw: \(attitude.yaw.truncate(places: 2)) roll: \(attitude.roll.truncate(places: 2))"
                
                self.infoLabel.text = str
                
                self.socketQueue.async {
                    // self._sendData(str)
                }
                
                // self.tone.setOffset( * 17, index: 1)
                self.tone.setOffset(attitude.roll * Double(arc4random() % 100), index: 0)
            }
        }
    }
    
    // MARK: - Send Data
    
    private func _packageData(_ data: CMAttitude) -> [String : Any] {
        return [
            "pitch" : data.pitch,
            "yaw" : data.yaw,
            "roll" : data.roll
        ]
    }
    
    private func _sendData(_ data: String) {
        socketLock.wait()
        
        if connected { return }
        
        switch socket.connect(timeout: 10) {
        case .success:
            connected = true
            // let result = socket.send(string: "pitch: \(data.pitch.truncate(places: 2)) yaw: \(data.yaw.truncate(places: 2)) roll: \(data.roll.truncate(places: 2))")
            let result = socket.send(string: data)
            print("Send Result: \(result)")
            let _ = socket.read(5)
            socket.close()
            connected = false
            socketLock.signal()
            break
        case .failure:
            socketLock.signal()
            break
        }
    }
}
