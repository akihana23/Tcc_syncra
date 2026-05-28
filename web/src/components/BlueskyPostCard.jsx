import {
  ExternalLink,
  Heart,
  MessageCircle,
  Repeat2,
} from 'lucide-react'

import SentimentBadge from './SentimentBadge'

function BlueskyPostCard({ post }) {
  return (
    <article className="
      rounded-lg
      border
      border-zinc-200
      bg-zinc-50
      p-5
      dark:border-zinc-800
      dark:bg-zinc-950
    ">
      <div className="
        mb-4
        flex
        items-start
        justify-between
        gap-4
      ">
        <h3 className="
          text-lg
          font-bold
          leading-snug
          text-zinc-950
          dark:text-white
        ">
          {post.title}
        </h3>

        <SentimentBadge
          sentiment={post.sentiment}
        />
      </div>

      <div className="
        mb-4
        flex
        flex-wrap
        gap-3
        text-sm
        text-zinc-500
        dark:text-zinc-400
      ">
        <span>{post.author || post.handle}</span>
        <span>@{post.handle}</span>
      </div>

      <div className="
        mb-4
        flex
        flex-wrap
        gap-4
        text-sm
        font-semibold
        text-zinc-700
        dark:text-zinc-200
      ">
        <span className="flex items-center gap-1">
          <Heart size={16} />
          {post.likes}
        </span>

        <span className="flex items-center gap-1">
          <MessageCircle size={16} />
          {post.replies}
        </span>

        <span className="flex items-center gap-1">
          <Repeat2 size={16} />
          {post.reposts}
        </span>
      </div>

      <a
        href={post.url}
        target="_blank"
        rel="noreferrer"
        className="
          inline-flex
          items-center
          gap-2
          text-sm
          font-bold
          text-sky-600
          hover:text-sky-700
        "
      >
        Ver post
        <ExternalLink size={15} />
      </a>
    </article>
  )
}

export default BlueskyPostCard
