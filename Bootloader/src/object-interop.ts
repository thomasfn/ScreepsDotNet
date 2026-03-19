const CLR_OBJECT_HANDLE = Symbol('clr-object-handle');

function hasId(obj: object): obj is { id: string } {
    // TODO: Are we going to have an issue here with pojo's with ids? e.g. an object from Memory which is just { id: 'xyz' }
    // We could address this by including an instanceof check here against known prototypes that go stale across ticks
    // e.g. World's GameObject
    return 'id' in obj;
}

export class ObjectInterop {
    private readonly _objectTrackingList: (object | undefined)[] = [];
    private readonly _objectTrackingListById: Map<string, object> = new Map();
    private readonly _nonExtensibleObjectTrackingMap: WeakMap<object, number> = new WeakMap();
    private readonly _freeObjectHandleList: number[] = [];

    private _nextObjectHandle: number = 0;

    private _numBeginTrackingObjects: number = 0;
    private _numReleaseTrackingObjects: number = 0;
    private _numTotalTrackingObjects: number = 0;

    public get numBeginTrackingObjects() { return this._numBeginTrackingObjects; }
    public get numReleaseTrackingObjects() { return this._numReleaseTrackingObjects; }
    public get numTotalTrackingObjects() { return this._numTotalTrackingObjects; }

    public loop(): void {
        this._numBeginTrackingObjects = 0;
        this._numReleaseTrackingObjects = 0;
    }

    public releaseObjectHandle(objectHandle: number): void {
        const obj = this._objectTrackingList[objectHandle];
        if (obj == null) { return; }
        this._objectTrackingList[objectHandle] = undefined;
        if (hasId(obj) && this._objectTrackingListById.get(obj.id) === obj) {
            this._objectTrackingListById.delete(obj.id);
        }
        this.clearObjectHandle(obj);
        ++this._numReleaseTrackingObjects;
        --this._numTotalTrackingObjects;
        this._freeObjectHandleList.push(objectHandle);
    }

    public getObjectByHandle(objectHandle: number): object | undefined {
        return this._objectTrackingList[objectHandle];
    }

    public getObjectHandle(obj: object): number | undefined {
        return (obj as { [CLR_OBJECT_HANDLE]: number | undefined })[CLR_OBJECT_HANDLE] ?? this._nonExtensibleObjectTrackingMap.get(obj);
    }

    private allocateObjectHandle(): number {
        return this._freeObjectHandleList.pop() ?? this._nextObjectHandle++;
    }

    private assignObjectHandle(obj: object, newObjectHandle?: number): number {
        if (newObjectHandle == null) {
            newObjectHandle = this.allocateObjectHandle();
            ++this._numBeginTrackingObjects;
            ++this._numTotalTrackingObjects;
        }
        if (Object.isExtensible(obj)) {
            (obj as { [CLR_OBJECT_HANDLE]: number | undefined })[CLR_OBJECT_HANDLE] = newObjectHandle;
        } else {
            this._nonExtensibleObjectTrackingMap.set(obj, newObjectHandle);
        }
        this._objectTrackingList[newObjectHandle] = obj;
        if (hasId(obj)) {
            this._objectTrackingListById.set(obj.id, obj);
        }
        return newObjectHandle;
    }

    private clearObjectHandle(obj: object): void {
        if (Object.isExtensible(obj)) {
            (obj as { [CLR_OBJECT_HANDLE]: number | undefined })[CLR_OBJECT_HANDLE] = undefined;
        } else {
            this._nonExtensibleObjectTrackingMap.delete(obj);
        }
    }

    public replaceObject(oldObj: object, newObj: object): number | undefined;

    public replaceObject(objectHandle: number, newObj: object): number;

    public replaceObject(p0: object | number, newObj: object): number | undefined {
        const objectHandle = typeof p0 === 'number' ? p0 : this.getObjectHandle(p0);
        if (objectHandle == null) { return; }
        const oldObj = typeof p0 === 'number' ? this._objectTrackingList[objectHandle] : p0;
        if (oldObj != null) {
            this.clearObjectHandle(oldObj);
        }
        return this.assignObjectHandle(newObj, objectHandle);
    }

    public getOrAssignObjectHandle(obj: object): number {
        let objectHandle = this.getObjectHandle(obj);
        if (objectHandle == null) {
            // It doesn't - if it has an id, see if we're already tracking a stale version of the game object
            if (hasId(obj)) {
                let previousVersion = this._objectTrackingListById.get(obj.id);
                if (previousVersion != null && previousVersion !== obj) {
                    // Replace the previous version with this one and reuse the tracking id
                    objectHandle = this.replaceObject(previousVersion, obj);
                }
            }
        }
        return objectHandle ?? this.assignObjectHandle(obj);
    }

    public visitTrackedObjects(visitor: (obj: unknown) => void): void {
        for (let i = 0; i < this._nextObjectHandle; ++i) {
            const obj = this._objectTrackingList[i];
            if (obj == null) { continue; }
            visitor(obj);
        }
    }
}
