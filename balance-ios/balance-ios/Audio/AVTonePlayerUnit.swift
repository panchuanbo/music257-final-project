//
//  AVTonePlayerUnit.swift
//  ToneGenerator
//
//  Created by OOPer in cooperation with shlab.jp, on 2015/3/22.
//  See LICENSE.txt .
//
//  Modified by Chuanbo Pan for MUSIC 257's Final Project
//
import Foundation
import AVFoundation

class AVTonePlayerUnit: AVAudioPlayerNode {
    let bufferCapacity: AVAudioFrameCount = 512
    let sampleRate: Double = 44_100.0
    
    var frequency: Double = 440.0
    var amplitude: Double = 0.25
    
    private var theta: [Double] = []
    private var audioFormat: AVAudioFormat!
    private var harmonics: Int = 1
    private var offsets: [Double] = []
    
    init(harmonics: Int = 1) {
        super.init()
        audioFormat = AVAudioFormat(standardFormatWithSampleRate: sampleRate, channels: 1)
        
        self.harmonics = harmonics
        
        for _ in 0..<harmonics {
            theta.append(0.0)
            offsets.append(0.0)
        }
    }
    
    func prepareBuffer() -> AVAudioPCMBuffer {
        let buffer = AVAudioPCMBuffer(pcmFormat: audioFormat, frameCapacity: bufferCapacity)!
        fillBuffer(buffer)
        return buffer
    }
    
    func fillBuffer(_ buffer: AVAudioPCMBuffer) {
        let data = buffer.floatChannelData?[0]
        let numberFrames = buffer.frameCapacity
        
        var theta = self.theta
        var theta_increments: [Double] = []
        
        for i in 0..<harmonics {
            theta_increments.append(Double(i + 1) * 2.0 * .pi * (self.frequency + offsets[i]) / self.sampleRate)
            
            //let multiplier = (i == 0) ? 1 : 1 / 1.25
            //theta_increments.append(multiplier * 2.0 * .pi * (self.frequency + offsets[i]) / self.sampleRate)
        }
        
        for frame in 0..<Int(numberFrames) {
            for i in 0..<harmonics {
                data?[frame] += Float32(sin(theta[i]) * amplitude / Double(harmonics))
                
                theta[i] += theta_increments[i]
                if theta[i] > 2.0 * .pi {
                    theta[i] -= 2.0 * .pi
                }
            }
        }
        buffer.frameLength = numberFrames
        self.theta = theta
    }
    
    func scheduleBuffer() {
        let buffer = prepareBuffer()
        self.scheduleBuffer(buffer) {
            if self.isPlaying {
                self.scheduleBuffer()
            }
        }
    }
    
    func preparePlaying() {
        scheduleBuffer()
        scheduleBuffer()
        scheduleBuffer()
        scheduleBuffer()
    }
    
    // MARK: - Set Offset
    
    public func setOffset(_ offset: Double, index: Int) {
        offsets[index] = offset
    }
}
