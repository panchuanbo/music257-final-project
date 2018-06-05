//
//  DoubleEx.swift
//  balance-ios
//
//  Created by Chuanbo Pan on 6/1/18.
//  Copyright © 2018 Chuanbo Pan. All rights reserved.
//

import UIKit

extension Double {
    func truncate(places: Int) -> Double {
        return Double(floor(pow(10.0, Double(places)) * self)/pow(10.0, Double(places)))
    }
}
