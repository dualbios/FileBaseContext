using kDg.FileBaseContext.Serializers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace FileBaseContext.Tests.Serializers
{
    [TestClass]
    public sealed class JsonRowDataConverterTests
    {
        #region Deserialize

        [TestMethod]
        public void DeserializeEmptyArray_NoRows()
        {
            var result = Deserialize(
                [
                    new("Column0", typeof(string)),
                ],
                "[]");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DeserializeRow_NoColumns_ReturnsRowWithDefaults()
        {
            var result = Deserialize(
                [
                    new("String", typeof(string)),
                    new("Int", typeof(int)),
                    new("NullableInt", typeof(int?)),
                ],
                "[{}]");

            Assert.AreEqual(1, result.Count);
            var row = result[0];
            Assert.AreEqual(3, row.Length);
            Assert.IsNull(row[0]);
            Assert.AreEqual(0, row[1]);
            Assert.IsNull(row[2]);
        }

        [TestMethod]
        public void DeserializeRow_OneMatchingColumn_ReturnsColumnData()
        {
            var result = Deserialize(
                [
                    new("Column0", typeof(string)),
                ],
                """
                [
                  {
                    "Column0": "Value0"
                  }
                ]
                """);

            Assert.AreEqual(1, result.Count);
            var row = result[0];
            Assert.AreEqual(1, row.Length);
            Assert.AreEqual("Value0", row[0]);
        }

        [TestMethod]
        public void DeserializeRow_MismatchedColumns_MatchingColumnsAreReturned()
        {
            var result = Deserialize(
                [
                    new("Column0", typeof(string)),
                    new("Column1", typeof(string)),
                    new("Column2", typeof(string)),
                ],
                """
                [
                  {
                    "Column3": "Value3",
                    "Column2": "Value2",
                    "Column0": "Value0"
                  }
                ]
                """);

            Assert.AreEqual(1, result.Count);
            var row = result[0];
            Assert.AreEqual(3, row.Length);
            Assert.AreEqual("Value0", row[0]);
            Assert.IsNull(row[1]);
            Assert.AreEqual("Value2", row[2]);
        }

        [TestMethod]
        public void DeserializeRows_MultipleRowsAreRead()
        {
            var result = Deserialize(
                [
                    new("Column0", typeof(string)),
                ],
                """
                [
                  {
                    "Column0": "Row0"
                  },
                  {
                    "Column0": "Row1"
                  },
                  {
                    "Column0": "Row2"
                  }
                ]
                """);

            Assert.AreEqual(3, result.Count);

            CollectionAssert.AreEqual(
                new int[] { 1, 1, 1, },
                result.Select(row => row.Length).ToList());

            CollectionAssert.AreEqual(
                new string[] { "Row0", "Row1", "Row2", },
                result.Select(row => row[0]).ToList());
        }

        [TestMethod]
        public void DeserializeRow_DifferentiatesNullFromEmptyString()
        {
            var result = Deserialize(
                [
                    new("Column0", typeof(string)),
                ],
                """
                [
                  {
                    "Column0": null
                  },
                  {
                    "Column0": ""
                  }
                ]
                """);

            Assert.IsNull(result[0][0]);
            Assert.AreEqual(string.Empty, result[1][0]);
        }

        [TestMethod]
        public void DeserializeInt_FromNumberOrString()
        {
            var result = Deserialize(
                [
                    new("Value", typeof(int)),
                    new("Nullable", typeof(int?)),
                ],
                """
                [
                  {
                    "Nullable": null
                  },
                  {
                    "Value": 10,
                    "Nullable": 11
                  },
                  {
                    "Value": "20",
                    "Nullable": "21"
                  }
                ]
                """);

            Assert.AreEqual(0, result[0][0]);
            Assert.IsNull(result[0][1]);

            Assert.AreEqual(10, result[1][0]);
            Assert.AreEqual(11, result[1][1]);

            Assert.AreEqual(20, result[2][0]);
            Assert.AreEqual(21, result[2][1]);

            Assert.AreEqual(typeof(int), result[1][0].GetType());
            Assert.AreEqual(typeof(int), result[1][1].GetType());
        }
        
        [TestMethod]
        public void DeserializeBool_FromBooleanOrString()
        {
            var result = Deserialize(
                [
                    new("Value", typeof(bool)),
                    new("Nullable", typeof(bool?)),
                ],
                """
                [
                  {
                    "Nullable": null
                  },
                  {
                    "Value": false,
                    "Nullable": null
                  },
                  {
                    "Value": "true",
                    "Nullable": "false"
                  }
                ]
                """);

            Assert.AreEqual(false, result[0][0]);
            Assert.IsNull(result[0][1]);

            Assert.AreEqual(false, result[1][0]);
            Assert.IsNull(result[1][1]);

            Assert.AreEqual(true, result[2][0]);
            Assert.AreEqual(false, result[2][1]);

            Assert.AreEqual(typeof(bool), result[1][0].GetType());
            Assert.IsNull(result[1][1]?.GetType());
        }

        [TestMethod]
        public void DeserializeLong_FromNumberOrString()
        {
            var result = Deserialize(
                [
                    new("Value", typeof(long)),
                    new("Nullable", typeof(long?)),
                ],
                """
                [
                  {
                    "Nullable": null
                  },
                  {
                    "Value": 10,
                    "Nullable": 11
                  },
                  {
                    "Value": "20",
                    "Nullable": "21"
                  }
                ]
                """);

            Assert.AreEqual(0L, result[0][0]);
            Assert.IsNull(result[0][1]);

            Assert.AreEqual(10L, result[1][0]);
            Assert.AreEqual(11L, result[1][1]);

            Assert.AreEqual(20L, result[2][0]);
            Assert.AreEqual(21L, result[2][1]);

            Assert.AreEqual(typeof(long), result[1][0].GetType());
            Assert.AreEqual(typeof(long), result[1][1].GetType());
        }

        [TestMethod]
        public void DeserializeDateTime_FromString()
        {
            var result = Deserialize(
                [
                    new("Value", typeof(DateTime)),
                    new("Nullable", typeof(DateTime?)),
                ],
                """
                [
                  {
                    "Nullable": null
                  },
                  {
                    "Value": "2000-01-02 03:04:05.678",
                    "Nullable": "2000-01-02 03:04:05.678"
                  }
                ]
                """);

            Assert.AreEqual(DateTime.MinValue, result[0][0]);
            Assert.IsNull(result[0][1]);

            var expectedValue = new DateTime(2000, 01, 02, 03, 04, 05, 678, DateTimeKind.Unspecified);
            Assert.AreEqual(expectedValue, result[1][0]);
            Assert.AreEqual(expectedValue, result[1][1]);
            Assert.AreEqual(expectedValue.Kind, ((DateTime)result[1][0]).Kind);
            Assert.AreEqual(expectedValue.Kind, ((DateTime)result[1][1]).Kind);
        }

        [TestMethod]
        public void DeserializeDateTimeOffset_FromString()
        {
            var result = Deserialize(
                [
                    new("Value", typeof(DateTimeOffset)),
                    new("Nullable", typeof(DateTimeOffset?)),
                ],
                """
                [
                  {
                    "Nullable": null
                  },
                  {
                    "Value": "2000-01-02T03:04:05.678Z",
                    "Nullable": "2000-01-02T03:04:05.678Z"
                  },
                  {
                    "Value": "2000-01-02T03:04:05.678-08:00",
                    "Nullable": "2000-01-02T03:04:05.678-08:00"
                  }
                ]
                """);

            Assert.AreEqual(DateTimeOffset.MinValue, result[0][0]);
            Assert.IsNull(result[0][1]);

            var expectedValue1 = new DateTimeOffset(2000, 01, 02, 03, 04, 05, 678, TimeSpan.Zero);
            Assert.AreEqual(expectedValue1, result[1][0]);
            Assert.AreEqual(expectedValue1, result[1][1]);
            Assert.AreEqual(expectedValue1.Offset, ((DateTimeOffset)result[1][0]).Offset);
            Assert.AreEqual(expectedValue1.Offset, ((DateTimeOffset)result[1][1]).Offset);

            var expectedValue2 = new DateTimeOffset(2000, 01, 02, 03, 04, 05, 678, TimeSpan.FromHours(-8));
            Assert.AreEqual(expectedValue2, result[2][0]);
            Assert.AreEqual(expectedValue2, result[2][1]);
            Assert.AreEqual(expectedValue2.Offset, ((DateTimeOffset)result[2][0]).Offset);
            Assert.AreEqual(expectedValue2.Offset, ((DateTimeOffset)result[2][1]).Offset);
        }

        [TestMethod]
        public void DeserializeTimeSpan_FromString()
        {
            var result = Deserialize(
                [
                    new("Value", typeof(TimeSpan)),
                    new("Nullable", typeof(TimeSpan?)),
                ],
                """
                [
                  {
                    "Nullable": null
                  },
                  {
                    "Value": "1.02:03:04.567",
                    "Nullable": "1.02:03:04.567"
                  }
                ]
                """);

            Assert.AreEqual(TimeSpan.Zero, result[0][0]);
            Assert.IsNull(result[0][1]);

            var expectedValue = new TimeSpan(1, 2, 3, 4, 567);
            Assert.AreEqual(expectedValue, result[1][0]);
            Assert.AreEqual(expectedValue, result[1][1]);
        }

        [TestMethod]
        public void DeserializeByteArray()
        {
            var result = Deserialize(
                [
                    new("Value", typeof(byte[])),
                ],
                """
                [
                  {
                    "Value": null
                  },
                  {
                    "Value": []
                  },
                  {
                    "Value": [0,1,2,3]
                  },
                  {
                    "Value": ""
                  },
                  {
                    "Value": "AAECAw=="
                  }
                ]
                """);

            Assert.IsNull(result[0][0]);
            CollectionAssert.AreEqual((byte[])[], (byte[])result[1][0]);
            CollectionAssert.AreEqual((byte[])[0, 1, 2, 3], (byte[])result[2][0]);
            CollectionAssert.AreEqual((byte[])[], (byte[])result[3][0]);
            CollectionAssert.AreEqual((byte[])[0, 1, 2, 3], (byte[])result[4][0]);
        }

        private static List<object[]> Deserialize(
            IEnumerable<JsonColumnInfo> columns,
            [StringSyntax(StringSyntaxAttribute.Json)] string json)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var options = JsonRowDataSerializer.CreateJsonOptions(columns);

            var rowsData = JsonSerializer.Deserialize<List<JsonRowData>>(stream, options);
            return rowsData!.Select(o => o.ColumnValues).ToList();
        }

        #endregion

        #region Serialize

        [TestMethod]
        public void SerializeEmpty()
        {
            var json = Serialize(
                [
                    new("Column0", typeof(string)),
                ],
                [
                ]);

            TestHelpers.AssertString(
                "[]",
                json);
        }

        [TestMethod]
        public void SerializeNoColumns()
        {
            var json = Serialize(
                [
                ],
                [
                    [],
                ]);

            TestHelpers.AssertString(
                 "[\r\n  {}\r\n]",
                 json);
        }

        [TestMethod]
        public void SerializeString()
        {
            var json = Serialize(
                [
                    new("Value", typeof(string)),
                ],
                [
                    [null],
                    [string.Empty],
                    ["String1"]
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": null\r\n  },\r\n  {\r\n    \"Value\": \"\"\r\n  },\r\n  {\r\n    \"Value\": \"String1\"\r\n  }\r\n]",
                json);
        }

        [TestMethod]
        public void SerializeInt()
        {
            var json = Serialize(
                [
                    new("Value", typeof(int)),
                    new("Nullable", typeof(int?)),
                ],
                [
                    [0, null],
                    [10, 11],
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": 0,\r\n    \"Nullable\": null\r\n  },\r\n  {\r\n    \"Value\": 10,\r\n    \"Nullable\": 11\r\n  }\r\n]",
                json);
        }

        [TestMethod]
        public void SerializeBool()
        {
            var json = Serialize(
                [
                    new("Value", typeof(bool)),
                    new("Nullable", typeof(bool?)),
                ],
                [
                    [true, null],
                    [false, true],
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": true,\r\n    \"Nullable\": null\r\n  },\r\n  {\r\n    \"Value\": false,\r\n    \"Nullable\": true\r\n  }\r\n]",
                json);
        }

        [TestMethod]
        public void SerializeLong()
        {
            var json = Serialize(
                [
                    new("Value", typeof(long)),
                    new("Nullable", typeof(long?)),
                ],
                [
                    [0L, null],
                    [10L, 11L],
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": 0,\r\n    \"Nullable\": null\r\n  },\r\n  {\r\n    \"Value\": 10,\r\n    \"Nullable\": 11\r\n  }\r\n]",
                json);
        }

        [TestMethod]
        public void SerializeDateTime()
        {
            var value = new DateTime(2000, 01, 02, 03, 04, 05, 678, DateTimeKind.Unspecified);
            var json = Serialize(
                [
                    new("Value", typeof(DateTime)),
                    new("Nullable", typeof(DateTime?)),
                ],
                [
                    [default(DateTime), null],
                    [value, value],
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": \"0001-01-01T00:00:00\",\r\n    \"Nullable\": null\r\n  },\r\n  {\r\n    \"Value\": \"2000-01-02T03:04:05.678\",\r\n    \"Nullable\": \"2000-01-02T03:04:05.678\"\r\n  }\r\n]",
                json);
        }

        [TestMethod]
        public void SerializeDateTimeOffset()
        {
            var valueUtc = new DateTimeOffset(2000, 01, 02, 03, 04, 05, 678, TimeSpan.Zero);
            var valueLocal = new DateTimeOffset(2000, 01, 02, 03, 04, 05, 678, TimeSpan.FromHours(-8));

            var json = Serialize(
                [
                    new("Value", typeof(DateTimeOffset)),
                    new("Nullable", typeof(DateTimeOffset?)),
                ],
                [
                    [default(DateTimeOffset), null],
                    [valueUtc, valueUtc],
                    [valueLocal, valueLocal],
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": \"0001-01-01T00:00:00+00:00\",\r\n    \"Nullable\": null\r\n  },\r\n  {\r\n    \"Value\": \"2000-01-02T03:04:05.678+00:00\",\r\n    \"Nullable\": \"2000-01-02T03:04:05.678+00:00\"\r\n  },\r\n  {\r\n    \"Value\": \"2000-01-02T03:04:05.678-08:00\",\r\n    \"Nullable\": \"2000-01-02T03:04:05.678-08:00\"\r\n  }\r\n]",
                json);
        }

        [TestMethod]
        public void SerializeTimeSpan()
        {
            var value = new TimeSpan(1, 02, 03, 04, 567);

            var json = Serialize(
                [
                    new("Value", typeof(TimeSpan)),
                    new("Nullable", typeof(TimeSpan?)),
                ],
                [
                    [default(TimeSpan), null],
                    [value, value],
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": \"00:00:00\",\r\n    \"Nullable\": null\r\n  },\r\n  {\r\n    \"Value\": \"1.02:03:04.5670000\",\r\n    \"Nullable\": \"1.02:03:04.5670000\"\r\n  }\r\n]",
                json);
        }

        [TestMethod]
        public void SerializeByteArray()
        {
            var json = Serialize(
                [
                    new("Value", typeof(byte[])),
                ],
                [
                    [null],
                    [(byte[])[]],
                    [(byte[])[0, 1, 2, 3]],
                ]);

            TestHelpers.AssertString(
                "[\r\n  {\r\n    \"Value\": null\r\n  },\r\n  {\r\n    \"Value\": \"\"\r\n  },\r\n  {\r\n    \"Value\": \"AAECAw==\"\r\n  }\r\n]",
                json);
        }

        //private void AssertStrings(string str, string json)
        //{
        //    if (str.Length != json.Length)
        //        Assert.Fail($"Strings have different length: {str.Length} != {json.Length}");

        //    for (int index = 0; index < str.Length; index++)
        //    {
        //        if (str[index] != json[index])
        //            Assert.Fail($"'{str[index]}' != '{json[index]}' at {index}.");
        //    }
        //}

        private static string Serialize(
            IEnumerable<JsonColumnInfo> columns,
            IEnumerable<object?[]> columnValues)
        {
            var options = JsonRowDataSerializer.CreateJsonOptions(columns);
            var rowsData = columnValues.Select(o => new JsonRowData(o)).ToList();
            return JsonSerializer.Serialize(rowsData, options);
        }

        #endregion
    }
}
