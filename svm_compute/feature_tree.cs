using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public class feature_tree
    {
        //public static List<(string full_name, string source, int source_id, List<(string full_name, string group, int group_id, List<(string full_name, string member, int member_id, List<(string full_name, string perspective, int perspective_id)> list)> list)> list)>


        public static void group_dataset_headers_all_levels(List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers)
        {
            //List<(string name, string source, List<(string name, string group, List<(string name, string member, List<(string name, string perspective)> list)> list)> list)>
            //
            //var dataset_headers_grouped = dataset_headers.Where(a => a.source != "class_id").GroupBy(a => a.source).Select(a => (name: a.Key, source: a.Key, list: a.GroupBy(b => (b.source, b.group)).Select(c => (name: $"{c.Key.source}.{c.Key.group}", group: c.Key.group, list: c.GroupBy(d => (d.source, d.group, d.member)).Select(e => (name: $"{e.Key.source}.{e.Key.group}.{e.Key.member}", member: e.Key.member, list: e.Select(g => (name: $"{g.source}.{g.group}.{g.member}.{g.perspective}", perspective: g.perspective)).ToList())).ToList())).ToList())).ToList();
            //
            //var dataset_headers_grouped =
            //
            //    dataset_headers/*.Where(a => a.fid != 0)*/
            //
            //        .GroupBy(a => a.source_id).Select(a => (full_name: a.FirstOrDefault().source, source: a.FirstOrDefault().source, source_id: a.Key, list:
            //
            //            a.GroupBy(b => (b.source_id, b.group_id)).Select(c => (full_name: $"{c.FirstOrDefault().source}.{c.FirstOrDefault().@group}", group: c.FirstOrDefault().@group, group_id: c.Key.group_id, list:
            //
            //                c.GroupBy(d => (d.source_id, d.group_id, d.member_id)).Select(e => (full_name: $"{e.FirstOrDefault().source}.{e.FirstOrDefault().@group}.{e.FirstOrDefault().member}", member: e.FirstOrDefault().member, member_id: e.Key.member_id, list:
            //
            //                    e.Select(g => (full_name: $"{g.source}.{g.@group}.{g.member}.{g.perspective}", perspective: g.perspective, perspective_id: g.perspective_id)).ToList())).ToList())).ToList())).ToList();


            var sources = dataset_headers.Select(a => a.source).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var groups = dataset_headers.Select(a => a.group).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var members = dataset_headers.Select(a => a.member).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var perspectives = dataset_headers.Select(a => a.perspective).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var alphabets = dataset_headers.Select(a => a.alphabet).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var categories = dataset_headers.Select(a => a.category).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var dimensions = dataset_headers.Select(a => a.dimension).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();

            var echo_source = true;
            var echo_group = true;
            var echo_dimension = true;
            var echo_category = true;
            var echo_alphabet = true;
            var echo_member = false;
            var echo_perspective = false;

            for (var source_index = 0; source_index < sources.Count; source_index++)
            {
                var source = sources[source_index];
                var source_items = dataset_headers.Where(a => a.source == source).ToList();
                var is_last_source = sources[sources.Count - 1] == source;

                if (echo_source)
                {
                    if (program.write_console_log) program.WriteLine($@"{$@"{(is_last_source ? "└" : "├")}{new string('─', 3)}┬ {nameof(source)}: {source}".PadRight(125)}{$@"(source: ".PadRight(14)}{$@"{source_items.Count})".PadLeft(8)}          {source}");
                }
                
                //┬
                var dimensions2 = dimensions.Where(a => source_items.Any(b => b.dimension == a)).ToList();

                for (var dimension_index = 0; dimension_index < dimensions2.Count; dimension_index++)
                {
                    var dimension = dimensions2[dimension_index];
                    var dimension_items = source_items.Where(a => a.dimension == dimension).ToList();

                    if (dimension_items.Count == 0) continue;

                    var is_last_dimension =  dimensions2[dimensions2.Count - 1] == dimension;

                    if (echo_dimension)
                    {
                        if (program.write_console_log) program.WriteLine($@"{$"{(is_last_source ? " " : "│")}{new string(' ', 3)}{(is_last_dimension ? "└" : "├")}{new string('─', 3)}┬ {nameof(dimension)}: {dimension}".PadRight(125)}{$@"(dimension: ".PadRight(14)}{$"{dimension_items.Count})".PadLeft(8)}          {source}.{dimension}");
                    }


                    var categories2 = categories.Where(a => dimension_items.Any(b => b.category == a)).ToList();


                    for (var category_index = 0; category_index < categories2.Count; category_index++)
                    {
                        var category = categories2[category_index];
                        var category_items = dimension_items.Where(a => a.category == category).ToList();

                        if (category_items.Count == 0) continue;


                        var is_last_category =  categories2[categories2.Count - 1] == category;

                        if (echo_category)
                        {
                            if (program.write_console_log) program.WriteLine($@"{$"{(is_last_source ? " " : "│")}{new string(' ', 3)}{(is_last_dimension ? " " : "│")}{new string(' ', 3)}{(is_last_category ? "└" : "├")}{new string('─', 3)}┬ {nameof(category)}: {category}".PadRight(125)}{$@"(category: ".PadRight(14)}{$"{category_items.Count})".PadLeft(8)}          {source}.{dimension}.{category}");
                        }

                        var alphabets2 = alphabets.Where(a => category_items.Any(b => b.alphabet == a)).ToList();

                        for (var alphabet_index = 0; alphabet_index < alphabets2.Count; alphabet_index++)
                        {
                            var alphabet = alphabets2[alphabet_index];
                            var alphabet_items = category_items.Where(a => a.alphabet == alphabet).ToList();

                            if (alphabet_items.Count == 0) continue;

                            var is_last_alphabet = alphabets2[alphabets2.Count - 1] == alphabet;

                            if (echo_alphabet)
                            {
                                if (program.write_console_log) program.WriteLine($@"{$"{(is_last_source ? " " : "│")}{new string(' ', 3)}{(is_last_dimension ? " " : "│")}{new string(' ', 3)}{(is_last_category ? " " : "│")}{new string(' ', 3)}{(is_last_alphabet ? "└" : "├")}{new string('─', 3)}┬ {nameof(alphabet)}: {alphabet}".PadRight(125)}{$@"(alphabet: ".PadRight(14)}{$"{alphabet_items.Count})".PadLeft(8)}          {source}.{dimension}.{category}.{alphabet}");
                            }

                            var groups2 = groups.Where(a => alphabet_items.Any(b => b.group == a)).ToList();

                            for (var group_index = 0; group_index < groups2.Count; group_index++)
                            {
                                var @group = groups2[group_index];
                                var group_items = alphabet_items.Where(a => a.@group == @group).ToList();

                                if (group_items.Count == 0) continue;

                                var is_last_group =  groups2[groups2.Count - 1] == @group;


                                if (echo_group)
                                {
                                    if (program.write_console_log) program.WriteLine($@"{$"{(is_last_source ? " " : "│")}{new string(' ', 3)}{(is_last_dimension ? " " : "│")}{new string(' ', 3)}{(is_last_category ? " " : "│")}{new string(' ', 3)}{(is_last_alphabet ? " " : "│")}{new string(' ', 3)}{(is_last_group ? "└" : "├")}{new string('─', 3)}┬ {nameof(@group)}: {@group}".PadRight(125)}{$@"(group: ".PadRight(14)}{$"{group_items.Count})".PadLeft(8)}          {source}.{dimension}.{category}.{alphabet}.{@group}");
                                }

                                var members2 = members.Where(a => group_items.Any(b => b.member == a)).ToList();


                                for (var member_index = 0; member_index < members2.Count; member_index++)
                                {
                                    var member = members2[member_index];
                                    var member_items = group_items.Where(a => a.member == member).ToList();

                                    if (member_items.Count == 0) continue;

                                    var is_last_member =members2[members2.Count - 1] == member;

                                    if (echo_member)
                                    {
                                        if (program.write_console_log) program.WriteLine($@"{$"{(is_last_source ? " " : "│")}{new string(' ', 3)}{(is_last_dimension ? " " : "│")}{new string(' ', 3)}{(is_last_category ? " " : "│")}{new string(' ', 3)}{(is_last_alphabet ? " " : "│")}{new string(' ', 3)}{(is_last_group ? " " : "│")}{new string('─', 3)}{(is_last_member ? "└" : "├")}{new string('─', 3)}┬ {nameof(member)}: {member}".PadRight(125)}{$@"(member: ".PadRight(14)}{$"{member_items.Count})".PadLeft(8)}          {source}.{dimension}.{category}.{alphabet}.{@group}.{member}");
                                    }

                                    var perspectives2 = perspectives.Where(a => member_items.Any(b => b.perspective == a)).ToList();

                                    for (var perspective_index = 0; perspective_index < perspectives2.Count; perspective_index++)
                                    {
                                        var perspective = perspectives2[perspective_index];
                                        var perspective_items = member_items.Where(a => a.perspective == perspective).ToList();

                                        if (perspective_items.Count == 0) continue;

                                        var is_last_perspective = perspectives2[perspectives2.Count - 1] == perspective;

                                        if (echo_perspective)
                                        {
                                            if (program.write_console_log) program.WriteLine($@"{$"{(is_last_source ? " " : "│")}{new string(' ', 3)}{(is_last_dimension ? " " : "│")}{new string(' ', 3)}{(is_last_category ? " " : "│")}{new string(' ', 3)}{(is_last_alphabet ? " " : "│")}{new string(' ', 3)}{(is_last_group ? " " : "│")}{new string('─', 3)}{(is_last_member ? " " : "│")}{new string('─', 3)}{(is_last_perspective ? "└" : "├")}{new string('─', 3)}┬ {nameof(perspective)}: {perspective}".PadRight(125)}{$@"(perspective: ".PadRight(14)}{$"{perspective_items.Count})".PadLeft(8)}          {source}.{dimension}.{category}.{alphabet}.{@group}.{member}.{perspective}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            //  print out feature list
            // to select a minimal set to test with

            //List<(int name, int source_id, List<(string name, int group_id, List<(string name, int member_id, List<(string name, string perspective)> list)> list)> list)> dfds = dataset_headers_grouped;


            //Console.ReadLine();
            //return dataset_headers_grouped;
        }

        /*
        public static void output_feature_tree(List<(string full_name, string source, int source_id, List<(string full_name, string group, int group_id, List<(string full_name, string member, int member_id, List<(string full_name, string perspective, int perspective_id)> list)> list)> list)> dataset_headers_grouped,

            bool echo_source, bool echo_group, bool echo_member, bool echo_perspective)
        {
            
            var param_list = new List<(string key, string value)>() { (nameof(dataset_headers_grouped), dataset_headers_grouped.ToString()), };
            var tree_cl = 100;

            if (program.write_console_log) program.WriteLine($@"{nameof(output_feature_tree)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            

            for (var source_index = 0; source_index < dataset_headers_grouped.Count; source_index++)
            {

                var source = dataset_headers_grouped[source_index];
                var is_last_source = source_index == dataset_headers_grouped.Count - 1;

                if (echo_source)
                {
                    if (program.write_console_log) program.WriteLine((is_last_source ? "└" : "├") + new string('─', 3) + " source: " + source.source);
                    Console.SetCursorPosition(tree_cl, Console.CursorTop - 1);
                    if (program.write_console_log) program.WriteLine(source.full_name, false);
                }

                for (var group_index = 0; group_index < source.list.Count; group_index++)
                {
                    var @group = source.list[group_index];
                    var is_last_group = group_index == source.list.Count - 1;

                    if (echo_group)
                    {
                        if (program.write_console_log) program.WriteLine((is_last_source ? " " : "│") + new string(' ', 3) + (is_last_group ? "└" : "├") + new string('─', 3) + " group: " + @group.@group);
                        Console.SetCursorPosition(tree_cl, Console.CursorTop - 1);
                        if (program.write_console_log) program.WriteLine(@group.full_name.Replace(".", "\t\t\t\t"), false);
                    }

                    for (var member_index = 0; member_index < @group.list.Count; member_index++)
                    {
                        var member = @group.list[member_index];
                        var is_last_member = member_index == @group.list.Count - 1;

                        if (echo_member)
                        {
                            if (program.write_console_log) program.WriteLine((is_last_source ? " " : "│") + new string(' ', 3) + (is_last_group ? " " : "│") + new string(' ', 3) + (is_last_member ? "└" : "├") + new string('─', 3) + " member: " + member.member);
                            Console.SetCursorPosition(tree_cl, Console.CursorTop - 1);
                            if (program.write_console_log) program.WriteLine(member.full_name.Replace(".", "\t\t\t\t"), false);
                        }

                        for (var perspective_index = 0; perspective_index < member.list.Count; perspective_index++)
                        {
                            var perspective = member.list[perspective_index];
                            var is_last_perspective = perspective_index == member.list.Count - 1;

                            if (echo_perspective)
                            {
                                if (program.write_console_log) program.WriteLine((is_last_source ? " " : "│") + new string(' ', 3) + (is_last_group ? " " : "│") + new string(' ', 3) + (is_last_member ? " " : "│") + new string(' ', 3) + (is_last_perspective ? "└" : "├") + new string('─', 3) + " perspective: " + perspective.perspective);
                                Console.SetCursorPosition(tree_cl, Console.CursorTop - 1);
                                if (program.write_console_log) program.WriteLine(perspective.full_name.Replace(".", "\t\t\t\t"), false);
                            }
                        }
                    }
                }
            }
        }
        */
    }
}
