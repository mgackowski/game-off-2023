/**
 * Can be scanned by a scanner.
 */
public interface IScannable
{
    bool IsLocked();
    bool IsScannedOnce();
    void Scan();

}