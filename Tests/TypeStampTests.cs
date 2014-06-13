﻿// *******************************************************************
//
//  Copyright (c) 2011-2014, Antmicro Ltd <antmicro.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// *******************************************************************
using System;
using NUnit.Framework;
using System.Linq;

namespace Antmicro.Migrant.Tests
{
    [TestFixture]
    public class TypeStampTests
    {
        [Test]
        public void ShouldFindNoDifferences()
        {
            var obj = DynamicClass.Create("C", DynamicClass.Create("B", DynamicClass.Create("A"))).Instantiate();
            var stamp = new TypeStamp(obj.GetType());

            var compareResult = stamp.CompareWith(stamp);
            Assert.IsTrue(compareResult.Empty);
        }

        [Test]
        public void ShouldDetectFieldInsertionSimple()
        {
            var objPrev = DynamicClass.Create("A").Instantiate();
            var objCurr = DynamicClass.Create("A").WithField("a", typeof(int)).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.FieldsAdded.Count);
            Assert.AreEqual("a", compareResult.FieldsAdded[0].Name);
        }

        [Test]
        public void ShouldNotDetectInsertionOfTransientField()
        {
            var objPrev = DynamicClass.Create("A").Instantiate();
            var objCurr = DynamicClass.Create("A").WithTransientField("a", typeof(int)).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsTrue(compareResult.Empty);
        }

        [Test]
        public void ShouldDetectInsertionOfOverridingField()
        {
            var objPrev = DynamicClass.Create("A", DynamicClass.Create("Base").WithField("a", typeof(int))).Instantiate();
            var objCurr = DynamicClass.Create("A", DynamicClass.Create("Base").WithField("a", typeof(int))).WithField("a", typeof(int)).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.FieldsAdded.Count);
            Assert.AreEqual("a", compareResult.FieldsAdded[0].Name);
        }

        [Test]
        public void ShouldDetectFieldRemovalSimple()
        {
            var objPrev = DynamicClass.Create("A").WithField("a", typeof(int)).Instantiate();
            var objCurr = DynamicClass.Create("A").Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.FieldsRemoved.Count);
            Assert.AreEqual("a", compareResult.FieldsRemoved[0].Name);
        }

        [Test]
        public void ShouldNotDetectRemovalOfTransientField()
        {
            var objPrev = DynamicClass.Create("A").WithTransientField("a", typeof(int)).Instantiate();
            var objCurr = DynamicClass.Create("A").Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsTrue(compareResult.Empty);
        }

        [Test]
        public void ShouldDetectRemovalOfOverridingField()
        {
            var objPrev = DynamicClass.Create("A", DynamicClass.Create("Base").WithField("a", typeof(int))).WithField("a", typeof(int)).Instantiate();
            var objCurr = DynamicClass.Create("A", DynamicClass.Create("Base").WithField("a", typeof(int))).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.FieldsRemoved.Count);
            Assert.AreEqual("a", compareResult.FieldsRemoved[0].Name);
        }

        [Test]
        public void ShouldDetectFieldMoveDownSimple()
        {
            var objPrev = DynamicClass.Create("A", DynamicClass.Create("Base")).WithField("a", typeof(int)).Instantiate();
            var objCurr = DynamicClass.Create("A", DynamicClass.Create("Base").WithField("a", typeof(int))).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.FieldsMoved.Count);
            Assert.AreEqual("a", compareResult.FieldsMoved.ElementAt(0).Key.Name);
        }

        [Test]
        public void ShouldDetectFieldMoveUpSimple()
        {
            var objPrev = DynamicClass.Create("A", DynamicClass.Create("Base")).WithField("a", typeof(int)).Instantiate();
            var objCurr = DynamicClass.Create("A", DynamicClass.Create("Base").WithField("a", typeof(int))).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.FieldsMoved.Count);
            Assert.AreEqual("a", compareResult.FieldsMoved.ElementAt(0).Key.Name);
        }

        [Test]
        public void ShouldDetectBaseClassInsertionSimple()
        {
            var objPrev = DynamicClass.Create("A").Instantiate();
            var objCurr = DynamicClass.Create("A", DynamicClass.Create("Base")).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.ClassesAdded.Count);
            Assert.IsTrue(compareResult.ClassesAdded[0].StartsWith("Base"));
        }

        [Test]
        public void ShouldDetectBaseClassInsertionComplex()
        {
            var objPrev = DynamicClass.Create("B", DynamicClass.Create("A")).Instantiate();
            var objCurr = DynamicClass.Create("B", 
                DynamicClass.Create("W",
                    DynamicClass.Create("Z",
                        DynamicClass.Create("A",
                            DynamicClass.Create("X",
                                DynamicClass.Create("O")))))).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(4, compareResult.ClassesAdded.Count);
            Assert.IsTrue(compareResult.ClassesAdded[0].StartsWith("O"));
            Assert.IsTrue(compareResult.ClassesAdded[1].StartsWith("X"));
            Assert.IsTrue(compareResult.ClassesAdded[2].StartsWith("Z"));
            Assert.IsTrue(compareResult.ClassesAdded[3].StartsWith("W"));
        }

        [Test]
        public void ShouldDetectBaseClassRemovalComplex()
        {
            var objPrev = DynamicClass.Create("B", 
                DynamicClass.Create("W",
                    DynamicClass.Create("Z",
                        DynamicClass.Create("A",
                            DynamicClass.Create("X",
                                DynamicClass.Create("O")))))).Instantiate();
            var objCurr = DynamicClass.Create("B", DynamicClass.Create("A")).Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(4, compareResult.ClassesRemoved.Count);
            Assert.IsTrue(compareResult.ClassesRemoved[0].StartsWith("O"));
            Assert.IsTrue(compareResult.ClassesRemoved[1].StartsWith("X"));
            Assert.IsTrue(compareResult.ClassesRemoved[2].StartsWith("Z"));
            Assert.IsTrue(compareResult.ClassesRemoved[3].StartsWith("W"));
        }

        [Test]
        public void ShouldDetectBaseClassRemovalSimple()
        {
            var objPrev = DynamicClass.Create("A", DynamicClass.Create("Base")).Instantiate();
            var objCurr = DynamicClass.Create("A").Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRenamed);
            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.ClassesRemoved.Count);
            Assert.IsTrue(compareResult.ClassesRemoved[0].StartsWith("Base"));
        }

        [Test]
        public void ShouldDetectClassRenameSimple()
        {
            var objPrev = DynamicClass.Create("A").Instantiate();
            var objCurr = DynamicClass.Create("B").Instantiate();

            var stampPrev = new TypeStamp(objPrev.GetType());
            var stampCurr = new TypeStamp(objCurr.GetType());

            var compareResult = stampCurr.CompareWith(stampPrev);

            Assert.IsEmpty(compareResult.ClassesAdded);
            Assert.IsEmpty(compareResult.ClassesRemoved);
            Assert.IsEmpty(compareResult.FieldsMoved);
            Assert.IsEmpty(compareResult.FieldsRemoved);
            Assert.IsEmpty(compareResult.FieldsAdded);
            Assert.IsEmpty(compareResult.FieldsChanged);

            Assert.AreEqual(1, compareResult.ClassesRenamed.Count);
            Assert.IsTrue(compareResult.ClassesRenamed[0].Item1.StartsWith("A"));
            Assert.IsTrue(compareResult.ClassesRenamed[0].Item2.StartsWith("B"));
        }
    }
}

