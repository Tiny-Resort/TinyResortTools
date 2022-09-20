using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace I2.Loc
{
	public class HindiFixer
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Func<char, bool> _003C_003E9__0_0;

			internal bool _003CFix_003Eb__0_0(char x)
			{
				return x != '\0';
			}
		}

		internal static string Fix(string text)
		{
			char[] array = text.ToCharArray();
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == '\u093f' && !char.IsWhiteSpace(array[i - 1]) && array[i - 1] != 0)
				{
					array[i] = array[i - 1];
					array[i - 1] = '\u093f';
					flag = true;
				}
				if (i != array.Length - 1)
				{
					if (array[i] == 'इ' && array[i + 1] == '\u093c')
					{
						array[i] = 'ऌ';
						array[i + 1] = '\0';
						flag = true;
					}
					if (array[i] == '\u0943' && array[i + 1] == '\u093c')
					{
						array[i] = '\u0944';
						array[i + 1] = '\0';
						flag = true;
					}
					if (array[i] == '\u0901' && array[i + 1] == '\u093c')
					{
						array[i] = 'ॐ';
						array[i + 1] = '\0';
						flag = true;
					}
					if (array[i] == 'ऋ' && array[i + 1] == '\u093c')
					{
						array[i] = 'ॠ';
						array[i + 1] = '\0';
						flag = true;
					}
					if (array[i] == 'ई' && array[i + 1] == '\u093c')
					{
						array[i] = 'ॡ';
						array[i + 1] = '\0';
						flag = true;
					}
					if (array[i] == '\u093f' && array[i + 1] == '\u093c')
					{
						array[i] = '\u0962';
						array[i + 1] = '\0';
						flag = true;
					}
					if (array[i] == '\u0940' && array[i + 1] == '\u093c')
					{
						array[i] = '\u0963';
						array[i + 1] = '\0';
						flag = true;
					}
					if (array[i] == '।' && array[i + 1] == '\u093c')
					{
						array[i] = 'ऽ';
						array[i + 1] = '\0';
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return text;
			}
			string text2 = new string(array.Where(_003C_003Ec._003C_003E9__0_0 ?? (_003C_003Ec._003C_003E9__0_0 = _003C_003Ec._003C_003E9._003CFix_003Eb__0_0)).ToArray());
			if (text2 == text)
			{
				return text2;
			}
			text = text2;
			return text;
		}
	}
}
