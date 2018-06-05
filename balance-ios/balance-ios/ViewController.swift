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
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        
        _setupMotion()
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    
    // MARK: - Motion Management
    
    private func _setupMotion() {
        motionManager.startDeviceMotionUpdates(to: OperationQueue.main) { (data, error) in
            if let attitude = data?.attitude {
                self.infoLabel.text = "pitch: \(attitude.pitch.truncate(places: 2)) yaw: \(attitude.yaw.truncate(places: 2)) roll: \(attitude.roll.truncate(places: 2))"
                
                let str = "pitch: \(attitude.pitch.truncate(places: 2)) yaw: \(attitude.yaw.truncate(places: 2)) roll: \(attitude.roll.truncate(places: 2))"
                
                self.socketQueue.async {
                    self._sendData(str)
                }
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
