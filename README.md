# CSharpHyperLogLog
C# implementation of the HyperLogLog algorithm by Philippe Flajolet. I discovered this fascinating algorithm with [redis](https://github.com/antirez/redis/) 
and wanted to give an implementation a try. The code is by far not perfect.

It contains the _standard_ algorithm of Philippe Flajolet with some of improvements (64-bits hash function & linear couting for small estimates).
There is also the second algorithm _HyperLogLog++_, made by Google engineers Stefan Heule, Marc Nunkesser and Alexander Hall.

## References

* [The original paper](http://algo.inria.fr/flajolet/Publications/FlFuGaMe07.pdf)
* [The HLL++ Google paper](http://research.google.com/pubs/pub40671.html)
* [The corresponding appendix](http://goo.gl/iU8Ig)
* I took some inspiration from [stream-lib](https://github.com/addthis/stream-lib/blob/master/src/main/java/com/clearspring/analytics/stream/cardinality/HyperLogLogPlus.java) and from [prasanthj](https://github.com/prasanthj/hyperloglog/blob/master/src/main/java/hyperloglog/HyperLogLog.java). I hope they don't mind :P

## Usage

If, by miracle, someone would ever want to use my library, just clone the repository and compile the ```CSharpHyperLogLog``` project to a _ddl_ dynamic library.

After that, use the abstract class ```HyperLogLog``` and instanciate one of the two concrete classes ```HyperLogLogNormal``` or ```HyperLogLogPlusPlus```.
And you're good to go!

## TODO

A lot of stuff is still to do :
* Fix the _Merge_ of the HLL++ implementation
* Make HLL++ threadsafe
* Add a cached cardinality
* And just improve readability :D

## Credits

David Wobrock (_david.wobrock@gmail.com_)
