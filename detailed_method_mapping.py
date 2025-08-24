#!/usr/bin/env python3
"""
Detailed Test Method Mapping Analysis
Maps every Swift test method to its C# counterpart (if exists)
"""

import os
import re
from pathlib import Path
from collections import defaultdict

def extract_detailed_test_methods(file_path: str, is_swift: bool):
    """Extract detailed test method information"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        methods = []
        
        if is_swift:
            # Extract Swift test methods with more context
            pattern = r'(?:public\s+)?func\s+(test\w+)\s*\([^)]*\)(?:\s+async)?(?:\s+throws)?\s*\{'
            matches = re.finditer(pattern, content, re.MULTILINE)
            
            for match in matches:
                method_name = match.group(1)
                # Get the line number
                line_start = content[:match.start()].count('\n') + 1
                
                # Extract method body to understand what it tests
                brace_count = 0
                pos = match.end() - 1  # Start at the opening brace
                method_end = pos
                
                for i, char in enumerate(content[pos:], pos):
                    if char == '{':
                        brace_count += 1
                    elif char == '}':
                        brace_count -= 1
                        if brace_count == 0:
                            method_end = i + 1
                            break
                
                method_body = content[match.start():method_end]
                
                # Analyze what the method tests
                test_info = {
                    'name': method_name,
                    'line': line_start,
                    'body_length': len(method_body.split('\n')),
                    'tests_async': 'async' in method_body or 'await' in method_body,
                    'has_assertions': 'XCTAssert' in method_body or 'XCTAssertEqual' in method_body,
                    'mock_usage': 'mockUrlSession' in method_body or 'Mock' in method_body,
                    'error_testing': 'XCTAssertThrowsError' in method_body or 'throws' in method_name.lower(),
                }
                
                methods.append(test_info)
        
        else:  # C# 
            # Extract C# test methods with attributes
            pattern = r'\[(?:Test|UnityTest)[^\]]*\]\s*(?:public\s+)?(?:async\s+)?(Task|void|IEnumerator)\s+(\w*Test\w*)\s*\([^)]*\)'
            matches = re.finditer(pattern, content, re.MULTILINE | re.DOTALL)
            
            for match in matches:
                return_type = match.group(1)
                method_name = match.group(2)
                line_start = content[:match.start()].count('\n') + 1
                
                # Get method body
                brace_count = 0
                pos = match.end()
                # Find opening brace
                while pos < len(content) and content[pos] != '{':
                    pos += 1
                
                method_end = pos
                for i, char in enumerate(content[pos:], pos):
                    if char == '{':
                        brace_count += 1
                    elif char == '}':
                        brace_count -= 1
                        if brace_count == 0:
                            method_end = i + 1
                            break
                
                method_body = content[match.start():method_end] if method_end > pos else content[match.start():match.end() + 100]
                
                test_info = {
                    'name': method_name,
                    'line': line_start,
                    'return_type': return_type,
                    'body_length': len(method_body.split('\n')),
                    'is_async': return_type == 'Task' or 'async' in method_body,
                    'is_unity_test': '[UnityTest]' in method_body,
                    'has_assertions': 'Assert.' in method_body,
                    'mock_usage': 'Mock' in method_body,
                    'performance_test': '[Performance]' in method_body,
                    'error_testing': 'Throws' in method_body or 'Exception' in method_body,
                }
                
                methods.append(test_info)
                
        return methods
        
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return []

def normalize_test_name(name: str) -> str:
    """Normalize test names for comparison"""
    # Remove common prefixes and make lowercase
    name = name.lower()
    if name.startswith('test'):
        name = name[4:]
    
    # Remove common suffixes
    for suffix in ['_test', 'test', '_tests', 'tests']:
        if name.endswith(suffix):
            name = name[:-len(suffix)]
    
    # Remove underscores and normalize
    return ''.join(name.split('_'))

def find_matching_c_sharp_method(swift_method: dict, csharp_methods: list) -> dict:
    """Find the best matching C# method for a Swift method"""
    swift_normalized = normalize_test_name(swift_method['name'])
    
    # Direct name matching
    for cs_method in csharp_methods:
        cs_normalized = normalize_test_name(cs_method['name'])
        if swift_normalized == cs_normalized:
            return {'match_type': 'exact', 'method': cs_method}
    
    # Partial matching
    for cs_method in csharp_methods:
        cs_normalized = normalize_test_name(cs_method['name'])
        if swift_normalized in cs_normalized or cs_normalized in swift_normalized:
            return {'match_type': 'partial', 'method': cs_method}
    
    return None

def analyze_file_pair(swift_file: str, csharp_file: str, swift_methods: list, csharp_methods: list):
    """Analyze a pair of Swift and C# test files"""
    analysis = {
        'swift_file': swift_file,
        'csharp_file': csharp_file,
        'swift_methods': len(swift_methods),
        'csharp_methods': len(csharp_methods),
        'matched_methods': [],
        'unmatched_swift': [],
        'extra_csharp': [],
        'coverage_stats': {}
    }
    
    matched_cs_methods = set()
    
    # Match Swift methods to C# methods
    for swift_method in swift_methods:
        match = find_matching_c_sharp_method(swift_method, csharp_methods)
        if match:
            analysis['matched_methods'].append({
                'swift': swift_method,
                'csharp': match['method'], 
                'match_type': match['match_type']
            })
            matched_cs_methods.add(match['method']['name'])
        else:
            analysis['unmatched_swift'].append(swift_method)
    
    # Find extra C# methods not in Swift
    for cs_method in csharp_methods:
        if cs_method['name'] not in matched_cs_methods:
            analysis['extra_csharp'].append(cs_method)
    
    # Calculate coverage statistics
    analysis['coverage_stats'] = {
        'coverage_percentage': round((len(analysis['matched_methods']) / len(swift_methods)) * 100 if swift_methods else 0, 1),
        'enhancement_factor': round(len(csharp_methods) / len(swift_methods) if swift_methods else 0, 2),
        'exact_matches': len([m for m in analysis['matched_methods'] if m['match_type'] == 'exact']),
        'partial_matches': len([m for m in analysis['matched_methods'] if m['match_type'] == 'partial']),
        'missing_methods': len(analysis['unmatched_swift']),
        'enhancement_methods': len(analysis['extra_csharp'])
    }
    
    return analysis

def generate_detailed_mapping_report():
    """Generate detailed method-by-method mapping report"""
    
    # Key file pairs to analyze in detail
    key_pairs = [
        ('contract/GasTokenTests.swift', 'Contract/GasTokenTests.cs'),
        ('contract/NeoTokenTests.swift', 'Contract/NeoTokenTests.cs'), 
        ('contract/NefFileTests.swift', 'Contract/NefFileTests.cs'),
        ('crypto/ECKeyPairTests.swift', 'Crypto/ECKeyPairTests.cs'),
        ('crypto/NEP2Tests.swift', 'Crypto/NEP2Tests.cs'),
        ('wallet/AccountTests.swift', 'Wallet/AccountTests.cs'),
        ('transaction/TransactionBuilderTests.swift', 'Transaction/TransactionBuilderTests.cs'),
        ('script/ScriptBuilderTests.swift', 'Script/ScriptBuilderTests.cs'),
        ('serialization/BinaryReaderTests.swift', 'Serialization/BinaryReaderTests.cs')
    ]
    
    swift_base = "/home/neo/git/NeoUnity/NeoSwift/Tests/NeoSwiftTests/unit"
    csharp_base = "/home/neo/git/NeoUnity/Tests/Runtime"
    
    report = []
    report.append("# 🔍 DETAILED TEST METHOD MAPPING ANALYSIS")
    report.append("## Method-by-Method Swift → C# Test Comparison")
    report.append("")
    
    total_coverage_stats = {
        'total_swift_methods': 0,
        'total_matched_methods': 0,
        'total_enhancement_methods': 0
    }
    
    for swift_rel, csharp_rel in key_pairs:
        swift_path = os.path.join(swift_base, swift_rel)
        csharp_path = os.path.join(csharp_base, csharp_rel)
        
        if not os.path.exists(swift_path):
            print(f"Warning: Swift file not found: {swift_path}")
            continue
            
        if not os.path.exists(csharp_path):
            print(f"Warning: C# file not found: {csharp_path}")
            continue
        
        swift_methods = extract_detailed_test_methods(swift_path, is_swift=True)
        csharp_methods = extract_detailed_test_methods(csharp_path, is_swift=False)
        
        analysis = analyze_file_pair(swift_rel, csharp_rel, swift_methods, csharp_methods)
        
        # Update totals
        total_coverage_stats['total_swift_methods'] += len(swift_methods)
        total_coverage_stats['total_matched_methods'] += len(analysis['matched_methods'])
        total_coverage_stats['total_enhancement_methods'] += len(analysis['extra_csharp'])
        
        # Generate report section for this file pair
        coverage = analysis['coverage_stats']['coverage_percentage']
        status_emoji = "✅" if coverage >= 100 else "⚠️" if coverage >= 50 else "❌"
        
        report.append(f"## {status_emoji} {Path(swift_rel).stem}")
        report.append(f"**Files**: `{Path(swift_rel).name}` → `{Path(csharp_rel).name}`")
        report.append("")
        report.append(f"### 📊 Coverage Statistics")
        report.append(f"- **Swift Methods**: {analysis['swift_methods']}")
        report.append(f"- **C# Methods**: {analysis['csharp_methods']}")
        report.append(f"- **Coverage**: {coverage}% ({len(analysis['matched_methods'])}/{analysis['swift_methods']})")
        report.append(f"- **Enhancement Factor**: {analysis['coverage_stats']['enhancement_factor']}x")
        report.append(f"- **Exact Matches**: {analysis['coverage_stats']['exact_matches']}")
        report.append(f"- **Partial Matches**: {analysis['coverage_stats']['partial_matches']}")
        report.append(f"- **Missing Methods**: {analysis['coverage_stats']['missing_methods']}")
        report.append(f"- **Enhancement Methods**: {analysis['coverage_stats']['enhancement_methods']}")
        report.append("")
        
        if analysis['matched_methods']:
            report.append("### ✅ Matched Methods")
            for match in analysis['matched_methods']:
                match_symbol = "🎯" if match['match_type'] == 'exact' else "🔗"
                swift_info = f"Swift: {match['swift']['name']} (L{match['swift']['line']}, {match['swift']['body_length']} lines)"
                csharp_info = f"C#: {match['csharp']['name']} (L{match['csharp']['line']}, {match['csharp']['body_length']} lines)"
                report.append(f"- {match_symbol} **{match['match_type'].title()} Match**")
                report.append(f"  - {swift_info}")  
                report.append(f"  - {csharp_info}")
            report.append("")
        
        if analysis['unmatched_swift']:
            report.append("### ❌ Missing C# Methods")
            for method in analysis['unmatched_swift']:
                async_indicator = " (async)" if method['tests_async'] else ""
                error_indicator = " (error test)" if method['error_testing'] else ""
                report.append(f"- `{method['name']}`{async_indicator}{error_indicator} - {method['body_length']} lines")
            report.append("")
        
        if analysis['extra_csharp']:
            report.append("### 🚀 C# Enhancement Methods")
            for method in analysis['extra_csharp']:
                unity_indicator = " (Unity)" if method['is_unity_test'] else ""
                perf_indicator = " (Performance)" if method['performance_test'] else ""
                async_indicator = " (async)" if method['is_async'] else ""
                report.append(f"- `{method['name']}`{unity_indicator}{perf_indicator}{async_indicator} - {method['body_length']} lines")
            report.append("")
        
        report.append("---")
        report.append("")
    
    # Summary statistics
    overall_coverage = round((total_coverage_stats['total_matched_methods'] / total_coverage_stats['total_swift_methods']) * 100 if total_coverage_stats['total_swift_methods'] > 0 else 0, 1)
    overall_enhancement = round((total_coverage_stats['total_matched_methods'] + total_coverage_stats['total_enhancement_methods']) / total_coverage_stats['total_swift_methods'] if total_coverage_stats['total_swift_methods'] > 0 else 0, 2)
    
    report.append("## 🏁 DETAILED MAPPING SUMMARY")
    report.append(f"**Analyzed Files**: {len([p for p in key_pairs if os.path.exists(os.path.join(swift_base, p[0])) and os.path.exists(os.path.join(csharp_base, p[1]))])} file pairs")
    report.append(f"**Total Swift Methods Analyzed**: {total_coverage_stats['total_swift_methods']}")
    report.append(f"**Total Matched Methods**: {total_coverage_stats['total_matched_methods']}")
    report.append(f"**Total Enhancement Methods**: {total_coverage_stats['total_enhancement_methods']}")
    report.append(f"**Overall Coverage**: {overall_coverage}%")
    report.append(f"**Overall Enhancement Factor**: {overall_enhancement}x")
    
    return "\n".join(report)

if __name__ == "__main__":
    detailed_report = generate_detailed_mapping_report()
    
    with open("/home/neo/git/NeoUnity/DETAILED_METHOD_MAPPING.md", "w") as f:
        f.write(detailed_report)
    
    print(detailed_report)