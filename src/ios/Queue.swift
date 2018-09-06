//
//  Queue.swift
//  Iotize Monitoring
//
//  Created by dev@iotize.com on 05/09/2018.
//

import Foundation

public struct Queue<T> {
    
    private var queueArray = [T]()
    
    public var count: Int {
        return queueArray.count
    }
    
    public var isEmpty: Bool {
        return queueArray.isEmpty
    }
    
    public mutating func enqueue(_ element: T) {
        queueArray.append(element)
    }
    
    public mutating func dequeue() -> T? {
        if isEmpty {
            return nil
        }
        return queueArray.removeFirst()
    }
    
    public var first: T? {
        return queueArray.first
    }
}
